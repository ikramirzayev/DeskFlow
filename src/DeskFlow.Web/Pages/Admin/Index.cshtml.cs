using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Kullanici> _userManager;

        public IndexModel(AppDbContext db, UserManager<Kullanici> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public int SalonSayisi { get; set; }
        public int MasaSayisi { get; set; }
        public int AktifMasaSayisi { get; set; }
        public int OgrenciSayisi { get; set; }
        public int BugunkuRezervasyonSayisi { get; set; }
        public int SuAnDoluMasaSayisi { get; set; }
        public int DolulukYuzdesi { get; set; }
        public int BugunGelmeyenSayisi { get; set; }

        public async Task OnGetAsync()
        {
            SalonSayisi = await _db.Salonlar.CountAsync();
            MasaSayisi = await _db.Masalar.CountAsync();
            AktifMasaSayisi = await _db.Masalar.CountAsync(m => m.AktifMi);
            OgrenciSayisi = (await _userManager.GetUsersInRoleAsync(Roller.Ogrenci)).Count;

            var gunBaslangici = DateTime.Today.ToUniversalTime();
            var gunBitisi = DateTime.Today.AddDays(1).ToUniversalTime();
            var bugunku = _db.Rezervasyonlar
                .Where(r => r.Baslangic >= gunBaslangici && r.Baslangic < gunBitisi);

            // İptal edilenler bugünün yükünü göstermez, sayıya katılmaz.
            BugunkuRezervasyonSayisi = await bugunku.CountAsync(r => r.Durum != RezervasyonDurumu.Iptal);
            BugunGelmeyenSayisi = await bugunku.CountAsync(r => r.Durum == RezervasyonDurumu.Gelmedi);

            // Şu an dolu: şu anki saati kapsayan, iptal/gelmedi olmamış rezervasyonu olan masalar.
            // Aynı masada üst üste rezervasyon olamaz ama yine de Distinct ile her masa bir kez sayılır.
            var simdiUtc = DateTime.UtcNow;
            SuAnDoluMasaSayisi = await _db.Rezervasyonlar
                .Where(r => (r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif) &&
                            r.Baslangic <= simdiUtc && simdiUtc < r.Bitis)
                .Select(r => r.MasaId)
                .Distinct()
                .CountAsync();

            DolulukYuzdesi = AktifMasaSayisi == 0
                ? 0
                : (int)Math.Round(100.0 * SuAnDoluMasaSayisi / AktifMasaSayisi);
        }
    }
}
