using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DeskFlow.Web.Models;
using DeskFlow.Web.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DeskFlow.Web.Pages.Admin.Masalar;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Masa> Masa { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public int? SalonId { get; set; }

    public SelectList Salonlar { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Salonlar = new SelectList(_context.Salonlar, "Id", "Ad", SalonId);

        var sorgu = _context.Masalar.Include(m => m.Salon).AsQueryable();
        if (SalonId.HasValue)
        {
            sorgu = sorgu.Where(m => m.SalonId == SalonId);
        }
        Masa = await sorgu.ToListAsync();
    }
}
