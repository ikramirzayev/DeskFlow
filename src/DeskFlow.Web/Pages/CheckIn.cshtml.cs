using DeskFlow.Web.Models;
using DeskFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using DeskFlow.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages
{
    [Authorize]
    public class CheckInModel : PageModel
    {
        private readonly CheckInServisi _checkInServisi;
        private readonly UserManager<Kullanici> _userManager;
        private readonly AppDbContext _db;

        public CheckInModel(CheckInServisi checkInServisi, UserManager<Kullanici> userManager, AppDbContext db)
        {
            _checkInServisi = checkInServisi;
            _userManager = userManager;
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string? Kod { get; set; }
        public CheckInSonucu? Sonuc { get; private set; }
        public string? Mesaj { get; private set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var kullaniciId = _userManager.GetUserId(User)!;

            Sonuc = await _checkInServisi.CheckInYapAsync(kullaniciId, Kod ?? string.Empty);
            Mesaj = Sonuc.Mesaj;

            if (Sonuc.Durum == CheckInDurumu.ErkenGeldi)
            {
                var kod = (Kod ?? string.Empty).Trim();
                var simdiUtc = DateTime.UtcNow;

                var baslangic = await _db.Rezervasyonlar
                    .Where(r => r.KullaniciId == kullaniciId
                             && r.Durum == RezervasyonDurumu.Beklemede
                             && r.Masa.QrKod.ToLower() == kod.ToLower()
                             && r.Baslangic > simdiUtc)
                    .OrderBy(r => r.Baslangic)
                    .Select(r => (DateTime?)r.Baslangic)
                    .FirstOrDefaultAsync();

                if (baslangic is not null)
                {
                    var itibaren = baslangic.Value.AddMinutes(-CheckInServisi.OnceDakika).ToLocalTime();
                    Mesaj = $"Check-in {itibaren:HH:mm}'ten itibaren yapılabilir.";
                }
            }

            return Page();
        }
    }
}