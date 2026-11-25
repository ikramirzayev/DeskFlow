using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using DeskFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages
{
    [Authorize]
    public class RezervasyonlarimModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly RezervasyonServisi _rezervasyonServisi;
        private readonly UserManager<Kullanici> _userManager;

        public RezervasyonlarimModel(
            AppDbContext db,
            RezervasyonServisi rezervasyonServisi,
            UserManager<Kullanici> userManager)
        {
            _db = db;
            _rezervasyonServisi = rezervasyonServisi;
            _userManager = userManager;
        }

        public List<RezervasyonSatiri> Yaklasan { get; set; } = new();

        public List<RezervasyonSatiri> Gecmis { get; set; } = new();

        /// Sayfada gösterilecek bilgiler; saatler burada yerel saate çevrilmiş olarak tutulur.
        public class RezervasyonSatiri
        {
            public int Id { get; init; }
            public string SalonAdi { get; init; } = string.Empty;
            public int Kat { get; init; }
            public string MasaNo { get; init; } = string.Empty;
            public DateTime Baslangic { get; init; }
            public DateTime Bitis { get; init; }
            public RezervasyonDurumu Durum { get; init; }
            public bool IptalEdilebilir { get; init; }
        }

        public async Task OnGetAsync()
        {
            var kullaniciId = _userManager.GetUserId(User)!;
            var simdiUtc = DateTime.UtcNow;

            var rezervasyonlar = await _db.Rezervasyonlar
                .Include(r => r.Masa)
                .ThenInclude(m => m.Salon)
                .Where(r => r.KullaniciId == kullaniciId)
                .OrderBy(r => r.Baslangic)
                .ToListAsync();

            foreach (var r in rezervasyonlar)
            {
                var satir = new RezervasyonSatiri
                {
                    Id = r.Id,
                    SalonAdi = r.Masa.Salon.Ad,
                    Kat = r.Masa.Salon.Kat,
                    MasaNo = r.Masa.MasaNo,
                    Baslangic = r.Baslangic.ToLocalTime(),
                    Bitis = r.Bitis.ToLocalTime(),
                    Durum = r.Durum,
                    IptalEdilebilir = RezervasyonServisi.IptalEdilebilirMi(r, simdiUtc)
                };

                var devamEdiyor = r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif;
                if (devamEdiyor && r.Bitis > simdiUtc)
                {
                    Yaklasan.Add(satir);
                }
                else
                {
                    Gecmis.Add(satir);
                }
            }

            // Geçmiş listesinde en yenisi en üstte.
            Gecmis.Reverse();
        }

        public async Task<IActionResult> OnPostIptalAsync(int id)
        {
            var kullaniciId = _userManager.GetUserId(User)!;
            var sonuc = await _rezervasyonServisi.IptalEtAsync(id, kullaniciId);

            switch (sonuc)
            {
                case IptalSonucu.Tamam:
                    TempData["Basari"] = "Rezervasyonunuz iptal edildi.";
                    break;
                case IptalSonucu.IptalEdilemez:
                    TempData["Hata"] = "Başlamış ya da zaten kapanmış bir rezervasyon iptal edilemez.";
                    break;
                default:
                    // Başkasının rezervasyonu da "bulunamadı" gibi gösterilir; varlığı belli edilmez.
                    TempData["Hata"] = "Rezervasyon bulunamadı.";
                    break;
            }

            return RedirectToPage();
        }

        public static string DurumMetni(RezervasyonDurumu durum) => durum switch
        {
            RezervasyonDurumu.Beklemede => "Beklemede",
            RezervasyonDurumu.Aktif => "Aktif",
            RezervasyonDurumu.Tamamlandi => "Tamamlandı",
            RezervasyonDurumu.Iptal => "İptal edildi",
            RezervasyonDurumu.Gelmedi => "Gelinmedi",
            _ => durum.ToString()
        };

        public static string DurumRengi(RezervasyonDurumu durum) => durum switch
        {
            RezervasyonDurumu.Beklemede => "text-bg-warning",
            RezervasyonDurumu.Aktif => "text-bg-success",
            RezervasyonDurumu.Tamamlandi => "text-bg-secondary",
            RezervasyonDurumu.Iptal => "text-bg-danger",
            _ => "text-bg-dark"
        };
    }
}
