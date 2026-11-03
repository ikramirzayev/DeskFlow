using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DeskFlow.Web.Models;
using DeskFlow.Web.Data;

namespace DeskFlow.Web.Pages.Admin.Masalar;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Masa> Masa { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Masa = await _context.Masalar.ToListAsync();
    }
}
