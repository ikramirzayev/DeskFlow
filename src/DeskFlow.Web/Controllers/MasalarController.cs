using DeskFlow.Web.Data;
using DeskFlow.Web.Dtos;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Controllers;

[ApiController]
[Route("api/masalar")]
public class MasalarController : ControllerBase
{
    private readonly AppDbContext _db;

    public MasalarController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/masalar?salonId=1&prizli=true&sessiz=true  (herkese açık)
    [HttpGet]
    public async Task<ActionResult<List<MasaDto>>> Listele(int? salonId, bool? prizli, bool? sessiz)
    {
        var sorgu = _db.Masalar.AsNoTracking().AsQueryable();

        if (salonId is not null)
        {
            sorgu = sorgu.Where(m => m.SalonId == salonId);
        }
        if (prizli is not null)
        {
            sorgu = sorgu.Where(m => m.PrizliMi == prizli);
        }
        if (sessiz is not null)
        {
            sorgu = sorgu.Where(m => m.SessizBolgeMi == sessiz);
        }

        var masalar = await sorgu
            .OrderBy(m => m.SalonId)
            .ThenBy(m => m.MasaNo)
            .Select(m => new MasaDto
            {
                Id = m.Id,
                MasaNo = m.MasaNo,
                SalonId = m.SalonId,
                SalonAd = m.Salon.Ad,
                X = m.X,
                Y = m.Y,
                PrizliMi = m.PrizliMi,
                SessizBolgeMi = m.SessizBolgeMi,
                AktifMi = m.AktifMi
            })
            .ToListAsync();

        return Ok(masalar);
    }

    // GET /api/masalar/5  (herkese açık)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MasaDto>> Getir(int id)
    {
        var masa = await _db.Masalar
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MasaDto
            {
                Id = m.Id,
                MasaNo = m.MasaNo,
                SalonId = m.SalonId,
                SalonAd = m.Salon.Ad,
                X = m.X,
                Y = m.Y,
                PrizliMi = m.PrizliMi,
                SessizBolgeMi = m.SessizBolgeMi,
                AktifMi = m.AktifMi
            })
            .FirstOrDefaultAsync();

        if (masa is null)
        {
            return NotFound(new { mesaj = "Masa bulunamadı." });
        }

        return Ok(masa);
    }

    // POST /api/masalar  (sadece yönetici) -> 201 Created
    [Authorize(Roles = Roller.Yonetici)]
    [HttpPost]
    public async Task<ActionResult<MasaDto>> Ekle(MasaKaydetDto dto)
    {
        var hata = await DogrulaAsync(dto, mevcutMasaId: null);
        if (hata is not null)
        {
            return hata;
        }

        var masaNo = dto.MasaNo.Trim();
        var masa = new Masa
        {
            MasaNo = masaNo,
            SalonId = dto.SalonId,
            X = dto.X,
            Y = dto.Y,
            PrizliMi = dto.PrizliMi,
            SessizBolgeMi = dto.SessizBolgeMi,
            AktifMi = dto.AktifMi,
            // Web'deki masa ekleme sayfasıyla aynı QR kod biçimi.
            QrKod = $"DESKFLOW-S{dto.SalonId}-{masaNo}"
        };

        _db.Masalar.Add(masa);
        await _db.SaveChangesAsync();

        var sonuc = await SalonAdiylaDtoAsync(masa);
        return CreatedAtAction(nameof(Getir), new { id = masa.Id }, sonuc);
    }

    // PUT /api/masalar/5  (sadece yönetici)
    [Authorize(Roles = Roller.Yonetici)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MasaDto>> Guncelle(int id, MasaKaydetDto dto)
    {
        var masa = await _db.Masalar.FirstOrDefaultAsync(m => m.Id == id);
        if (masa is null)
        {
            return NotFound(new { mesaj = "Masa bulunamadı." });
        }

        var hata = await DogrulaAsync(dto, mevcutMasaId: id);
        if (hata is not null)
        {
            return hata;
        }

        // QR kod, masanın üzerine basılı olabileceği için değişmez.
        masa.MasaNo = dto.MasaNo.Trim();
        masa.SalonId = dto.SalonId;
        masa.X = dto.X;
        masa.Y = dto.Y;
        masa.PrizliMi = dto.PrizliMi;
        masa.SessizBolgeMi = dto.SessizBolgeMi;
        masa.AktifMi = dto.AktifMi;

        await _db.SaveChangesAsync();

        return Ok(await SalonAdiylaDtoAsync(masa));
    }

    // DELETE /api/masalar/5  (sadece yönetici) -> 204 No Content
    [Authorize(Roles = Roller.Yonetici)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var masa = await _db.Masalar.FirstOrDefaultAsync(m => m.Id == id);
        if (masa is null)
        {
            return NotFound(new { mesaj = "Masa bulunamadı." });
        }

        // Rezervasyon kayıtları korunur: rezervasyonu olan masa silinemez.
        if (await _db.Rezervasyonlar.AnyAsync(r => r.MasaId == id))
        {
            return Conflict(new { mesaj = "Bu masaya ait rezervasyonlar olduğu için masa silinemez. Masayı pasif yapabilirsiniz." });
        }

        _db.Masalar.Remove(masa);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Salon var mı ve aynı salonda aynı masa numarası kullanılmış mı? Sorun varsa 400 döner.
    private async Task<ActionResult?> DogrulaAsync(MasaKaydetDto dto, int? mevcutMasaId)
    {
        var masaNo = dto.MasaNo.Trim();

        if (!await _db.Salonlar.AnyAsync(s => s.Id == dto.SalonId))
        {
            ModelState.AddModelError(nameof(dto.SalonId), "Seçilen salon bulunamadı.");
        }
        else if (await _db.Masalar.AnyAsync(m =>
                     m.SalonId == dto.SalonId && m.MasaNo == masaNo && m.Id != mevcutMasaId))
        {
            ModelState.AddModelError(nameof(dto.MasaNo), $"Bu salonda {masaNo} numaralı masa zaten var.");
        }

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }

    private async Task<MasaDto> SalonAdiylaDtoAsync(Masa m)
    {
        var salonAd = await _db.Salonlar
            .Where(s => s.Id == m.SalonId)
            .Select(s => s.Ad)
            .FirstAsync();

        return new MasaDto
        {
            Id = m.Id,
            MasaNo = m.MasaNo,
            SalonId = m.SalonId,
            SalonAd = salonAd,
            X = m.X,
            Y = m.Y,
            PrizliMi = m.PrizliMi,
            SessizBolgeMi = m.SessizBolgeMi,
            AktifMi = m.AktifMi
        };
    }
}
