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

        public async Task OnGetAsync()
        {
            SalonSayisi = await _db.Salonlar.CountAsync();
            MasaSayisi = await _db.Masalar.CountAsync();
            AktifMasaSayisi = await _db.Masalar.CountAsync(m => m.AktifMi);
            OgrenciSayisi = (await _userManager.GetUsersInRoleAsync(Roller.Ogrenci)).Count;

            // Veritabanında zamanlar UTC tutuluyor; "bugün"ü yerel saate göre hesaplayıp UTC'ye çeviriyoruz.
            var gunBaslangici = DateTime.Today.ToUniversalTime();
            var gunBitisi = DateTime.Today.AddDays(1).ToUniversalTime();
            BugunkuRezervasyonSayisi = await _db.Rezervasyonlar
                .CountAsync(r => r.Baslangic >= gunBaslangici && r.Baslangic < gunBitisi);
        }
    }
}
