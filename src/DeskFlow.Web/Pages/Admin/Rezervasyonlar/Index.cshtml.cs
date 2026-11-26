using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using DeskFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages.Admin.Rezervasyonlar;

// Yetki: Program.cs'teki AuthorizeFolder("/Admin", "YoneticiPolitikasi") bu sayfayı da korur.
public class IndexModel : PageModel
{
    private const int EnFazlaSatir = 200;

    private readonly AppDbContext _db;
    private readonly RezervasyonServisi _rezervasyonServisi;

    public IndexModel(AppDbContext db, RezervasyonServisi rezervasyonServisi)
    {
        _db = db;
        _rezervasyonServisi = rezervasyonServisi;
    }

    // Filtreler adres çubuğundan gelir (?Tarih=2026-12-01&SalonId=1&Durum=Aktif),
    // böylece filtrelenmiş liste yenilenince ya da paylaşılınca aynı kalır.
    [BindProperty(SupportsGet = true)]
    public DateOnly? Tarih { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SalonId { get; set; }

    [BindProperty(SupportsGet = true)]
    public RezervasyonDurumu? Durum { get; set; }

    public SelectList Salonlar { get; set; } = default!;

    public List<SelectListItem> Durumlar { get; set; } = new();

    public List<Satir> Satirlar { get; set; } = new();

    public bool SinirAsildi { get; set; }

    public class Satir
    {
        public int Id { get; init; }
        public string AdSoyad { get; init; } = string.Empty;
        public string? OgrenciNo { get; init; }
        public string SalonAdi { get; init; } = string.Empty;
        public string MasaNo { get; init; } = string.Empty;
        public DateTime Baslangic { get; init; }
        public DateTime Bitis { get; init; }
        public RezervasyonDurumu Durum { get; init; }
        public bool IptalEdilebilir { get; init; }
    }

    public async Task OnGetAsync()
    {
        Salonlar = new SelectList(
            await _db.Salonlar.OrderBy(s => s.Ad).ToListAsync(), "Id", "Ad", SalonId);

        Durumlar = Enum.GetValues<RezervasyonDurumu>()
            .Select(d => new SelectListItem(d.Metin(), d.ToString(), d == Durum))
            .ToList();

        var sorgu = _db.Rezervasyonlar
            .AsNoTracking()
            .Include(r => r.Kullanici)
            .Include(r => r.Masa)
            .ThenInclude(m => m.Salon)
            .AsQueryable();

        if (Tarih is { } tarih)
        {
            // Seçilen gün yerel saatle; veritabanında UTC tutulduğu için aralığı UTC'ye çeviririz.
            var gunBaslangici = tarih.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
            var gunBitisi = tarih.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
            sorgu = sorgu.Where(r => r.Baslangic >= gunBaslangici && r.Baslangic < gunBitisi);
        }

        if (SalonId is { } salonId)
        {
            sorgu = sorgu.Where(r => r.Masa.SalonId == salonId);
        }

        if (Durum is { } durum)
        {
            sorgu = sorgu.Where(r => r.Durum == durum);
        }

        // Bir fazlasını çekip sınırın aşılıp aşılmadığını anlarız.
        var kayitlar = await sorgu
            .OrderByDescending(r => r.Baslangic)
            .Take(EnFazlaSatir + 1)
            .ToListAsync();

        SinirAsildi = kayitlar.Count > EnFazlaSatir;
        var simdiUtc = DateTime.UtcNow;

        Satirlar = kayitlar.Take(EnFazlaSatir).Select(r => new Satir
        {
            Id = r.Id,
            AdSoyad = $"{r.Kullanici.Ad} {r.Kullanici.Soyad}",
            OgrenciNo = r.Kullanici.OgrenciNo,
            SalonAdi = r.Masa.Salon.Ad,
            MasaNo = r.Masa.MasaNo,
            Baslangic = r.Baslangic.ToLocalTime(),
            Bitis = r.Bitis.ToLocalTime(),
            Durum = r.Durum,
            IptalEdilebilir = (r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif)
                      && r.Bitis > simdiUtc
        }).ToList();
    }

    public async Task<IActionResult> OnPostIptalAsync(int id)
    {
        var sonuc = await _rezervasyonServisi.YoneticiIptalAsync(id);

        if (sonuc == IptalSonucu.Tamam)
        {
            TempData["Basari"] = "Rezervasyon iptal edildi.";
        }
        else if (sonuc == IptalSonucu.Bulunamadi)
        {
            TempData["Hata"] = "Rezervasyon bulunamadı.";
        }
        else
        {
            TempData["Hata"] = "Bitmiş ya da zaten kapanmış bir rezervasyon iptal edilemez.";
        }

        // Filtreler kaybolmasın diye aynı sayfaya aynı filtrelerle dön.
        return RedirectToPage(new { Tarih = Tarih?.ToString("yyyy-MM-dd"), SalonId, Durum });
    }
}
