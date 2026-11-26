using System.Security.Claims;
using DeskFlow.Web.Data;
using DeskFlow.Web.Dtos;
using DeskFlow.Web.Models;
using DeskFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Controllers;

[ApiController]
[Route("api/rezervasyonlar")]
[Authorize]
public class RezervasyonlarController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly RezervasyonServisi _servis;

    public RezervasyonlarController(AppDbContext db, RezervasyonServisi servis)
    {
        _db = db;
        _servis = servis;
    }

    private string KullaniciId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET /api/rezervasyonlar/benim  -> giriş yapan kullanıcının rezervasyonları
    [HttpGet("benim")]
    public async Task<ActionResult<List<RezervasyonDto>>> Benim()
    {
        var simdiUtc = DateTime.UtcNow;
        var kayitlar = await _db.Rezervasyonlar
            .AsNoTracking()
            .Include(r => r.Masa).ThenInclude(m => m.Salon)
            .Where(r => r.KullaniciId == KullaniciId)
            .OrderByDescending(r => r.Baslangic)
            .ToListAsync();

        return Ok(kayitlar.Select(r => DtoyaCevir(r, simdiUtc)).ToList());
    }

    // POST /api/rezervasyonlar  -> 201 Created, kural hatasında 400, çakışmada 409
    [HttpPost]
    public async Task<ActionResult<RezervasyonDto>> Olustur(RezervasyonOlusturDto dto)
    {
        // Servis yerel saat bekler. "Z" ile UTC gelirse yerel saate çevrilir.
        var baslangic = YerelYap(dto.Baslangic!.Value);
        var bitis = YerelYap(dto.Bitis!.Value);

        var sonuc = await _servis.OlusturAsync(KullaniciId, dto.MasaId, baslangic, bitis);
        if (!sonuc.Basarili)
        {
            var hata = new { mesaj = sonuc.Hata };
            return sonuc.Cakisma ? Conflict(hata) : BadRequest(hata);
        }

        var kayit = await _db.Rezervasyonlar
            .AsNoTracking()
            .Include(r => r.Masa).ThenInclude(m => m.Salon)
            .FirstAsync(r => r.Id == sonuc.Rezervasyon!.Id);

        return Created($"/api/rezervasyonlar/benim", DtoyaCevir(kayit, DateTime.UtcNow));
    }

    // DELETE /api/rezervasyonlar/5  -> iptal (kayıt silinmez, durumu İptal olur)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> IptalEt(int id)
    {
        var sonuc = await _servis.IptalEtAsync(id, KullaniciId);

        return sonuc switch
        {
            IptalSonucu.Tamam => NoContent(),
            IptalSonucu.Bulunamadi => NotFound(new { mesaj = "Rezervasyon bulunamadı." }),
            IptalSonucu.YetkiYok => StatusCode(StatusCodes.Status403Forbidden,
                new { mesaj = "Bu rezervasyon size ait değil." }),
            _ => Conflict(new { mesaj = "Başlamış ya da zaten kapanmış bir rezervasyon iptal edilemez." })
        };
    }

    // GET /api/salonlar/1/doluluk?tarih=2026-11-27  -> o gün her masanın dolu saat aralıkları
    [HttpGet("~/api/salonlar/{id:int}/doluluk")]
    public async Task<ActionResult<DolulukDto>> Doluluk(int id, DateOnly? tarih)
    {
        var salon = await _db.Salonlar.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (salon is null)
        {
            return NotFound(new { mesaj = "Salon bulunamadı." });
        }

        var gun = tarih ?? DateOnly.FromDateTime(DateTime.Now);
        var gunBaslangicUtc = RezervasyonServisi.YereldenUtc(gun.ToDateTime(TimeOnly.MinValue));
        var gunBitisUtc = gunBaslangicUtc.AddDays(1);

        var masalar = await _db.Masalar
            .AsNoTracking()
            .Where(m => m.SalonId == id)
            .OrderBy(m => m.MasaNo)
            .ToListAsync();

        var dolu = await _db.Rezervasyonlar
            .AsNoTracking()
            .Where(r => r.Masa.SalonId == id &&
                        (r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif) &&
                        r.Baslangic < gunBitisUtc && gunBaslangicUtc < r.Bitis)
            .OrderBy(r => r.Baslangic)
            .ToListAsync();

        var sonuc = new DolulukDto
        {
            SalonId = salon.Id,
            SalonAd = salon.Ad,
            Tarih = gun,
            Masalar = masalar.Select(m => new MasaDolulukDto
            {
                MasaId = m.Id,
                MasaNo = m.MasaNo,
                DoluAraliklar = dolu
                    .Where(r => r.MasaId == m.Id)
                    .Select(r => new DoluAralikDto
                    {
                        Baslangic = r.Baslangic.ToLocalTime(),
                        Bitis = r.Bitis.ToLocalTime()
                    })
                    .ToList()
            }).ToList()
        };

        return Ok(sonuc);
    }

    private static DateTime YerelYap(DateTime zaman) =>
        zaman.Kind == DateTimeKind.Utc ? zaman.ToLocalTime() : zaman;

    private static RezervasyonDto DtoyaCevir(Rezervasyon r, DateTime simdiUtc) => new()
    {
        Id = r.Id,
        MasaId = r.MasaId,
        MasaNo = r.Masa.MasaNo,
        SalonId = r.Masa.SalonId,
        SalonAd = r.Masa.Salon.Ad,
        Kat = r.Masa.Salon.Kat,
        Baslangic = r.Baslangic.ToLocalTime(),
        Bitis = r.Bitis.ToLocalTime(),
        Durum = r.Durum.ToString(),
        IptalEdilebilir = RezervasyonServisi.IptalEdilebilirMi(r, simdiUtc)
    };
}
