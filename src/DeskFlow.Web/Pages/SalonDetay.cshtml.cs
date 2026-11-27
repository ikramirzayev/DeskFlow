using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages;

public class SalonDetayModel : PageModel
{
    private readonly AppDbContext _db;

    public SalonDetayModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Prizli { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Sessiz { get; set; }

    public Salon? Salon { get; set; }

    public List<Masa> Masalar { get; set; } = new();
    public Dictionary<int, RezervasyonDurumu> MasaDurumlari { get; set; } = new();
    public async Task<IActionResult> OnGetAsync()
    {
        Salon = await _db.Salonlar.FirstOrDefaultAsync(s => s.Id == Id && s.AktifMi);
        if (Salon == null)
        {
            return NotFound();
        }

        var sorgu = _db.Masalar.Where(m => m.SalonId == Id && m.AktifMi);
        if (Prizli)
        {
            sorgu = sorgu.Where(m => m.PrizliMi);
        }
        if (Sessiz)
        {
            sorgu = sorgu.Where(m => m.SessizBolgeMi);
        }

        Masalar = await sorgu.OrderBy(m => m.MasaNo).ToListAsync();
        var masaIdleri = Masalar.Select(m => m.Id).ToList();
        var simdi = DateTime.UtcNow;

        var simdikiler = await _db.Rezervasyonlar
    .Where(r => masaIdleri.Contains(r.MasaId)
        && (r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif)
        && r.Baslangic <= simdi && simdi < r.Bitis)
    .Select(r => new { r.MasaId, r.Durum })
    .ToListAsync();

        MasaDurumlari = simdikiler
            .GroupBy(x => x.MasaId)
            .ToDictionary(
                g => g.Key,
                g => g.Any(x => x.Durum == RezervasyonDurumu.Aktif)
                    ? RezervasyonDurumu.Aktif
                    : RezervasyonDurumu.Beklemede);
        return Page();
    }
}