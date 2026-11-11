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
        return Page();
    }
}