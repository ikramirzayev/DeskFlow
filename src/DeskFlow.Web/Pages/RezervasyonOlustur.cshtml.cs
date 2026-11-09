using System.ComponentModel.DataAnnotations;
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
    public class RezervasyonOlusturModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly RezervasyonServisi _rezervasyonServisi;
        private readonly UserManager<Kullanici> _userManager;

        public RezervasyonOlusturModel(
            AppDbContext db,
            RezervasyonServisi rezervasyonServisi,
            UserManager<Kullanici> userManager)
        {
            _db = db;
            _rezervasyonServisi = rezervasyonServisi;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int MasaId { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public Masa? Masa { get; set; }

        /// Bu masanın önümüzdeki günlerdeki dolu saatleri (yerel saatle).
        public List<(DateTime Baslangic, DateTime Bitis)> DoluSaatler { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Tarih zorunludur.")]
            [DataType(DataType.Date)]
            [Display(Name = "Tarih")]
            public DateOnly? Tarih { get; set; }

            [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
            [DataType(DataType.Time)]
            [Display(Name = "Başlangıç saati")]
            public TimeOnly? BaslangicSaati { get; set; }

            [Required(ErrorMessage = "Bitiş saati zorunludur.")]
            [DataType(DataType.Time)]
            [Display(Name = "Bitiş saati")]
            public TimeOnly? BitisSaati { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!await MasayiYukleAsync())
            {
                return NotFound();
            }

            // Varsayılan: bir sonraki tam saatten başlayan 2 saatlik rezervasyon.
            var sonrakiSaat = DateTime.Now.AddHours(1);
            var baslangic = new DateTime(sonrakiSaat.Year, sonrakiSaat.Month, sonrakiSaat.Day, sonrakiSaat.Hour, 0, 0);
            Input.Tarih = DateOnly.FromDateTime(baslangic);
            Input.BaslangicSaati = TimeOnly.FromDateTime(baslangic);
            Input.BitisSaati = TimeOnly.FromDateTime(baslangic.AddHours(2));

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await MasayiYukleAsync())
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var baslangic = Input.Tarih!.Value.ToDateTime(Input.BaslangicSaati!.Value);
            var bitis = Input.Tarih!.Value.ToDateTime(Input.BitisSaati!.Value);
            var kullaniciId = _userManager.GetUserId(User)!;

            var sonuc = await _rezervasyonServisi.OlusturAsync(kullaniciId, MasaId, baslangic, bitis);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
                return Page();
            }

            TempData["Basari"] =
                $"Rezervasyonunuz oluşturuldu: {Masa!.Salon.Ad}, masa {Masa.MasaNo}, " +
                $"{baslangic:dd.MM.yyyy} {baslangic:HH\\:mm} – {bitis:HH\\:mm}.";

            return RedirectToPage("/Index");
        }

        private async Task<bool> MasayiYukleAsync()
        {
            Masa = await _db.Masalar
                .Include(m => m.Salon)
                .FirstOrDefaultAsync(m => m.Id == MasaId);

            if (Masa is null)
            {
                return false;
            }

            var simdiUtc = DateTime.UtcNow;
            var sinirUtc = simdiUtc.AddDays(RezervasyonServisi.EnFazlaGunSonra + 1);

            var dolu = await _db.Rezervasyonlar
                .Where(r => r.MasaId == MasaId &&
                            (r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif) &&
                            r.Bitis > simdiUtc && r.Baslangic < sinirUtc)
                .OrderBy(r => r.Baslangic)
                .Select(r => new { r.Baslangic, r.Bitis })
                .ToListAsync();

            DoluSaatler = dolu
                .Select(r => (r.Baslangic.ToLocalTime(), r.Bitis.ToLocalTime()))
                .ToList();

            return true;
        }
    }
}
