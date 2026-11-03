using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DeskFlow.Web.Models;
using DeskFlow.Web.Data;

namespace DeskFlow.Web.Pages.Admin.Masalar;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;
    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public Masa Masa { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var masa = await _context.Masalar.FirstOrDefaultAsync(m => m.Id == id);
        if (masa is null)
        {
            return NotFound();
        }
        else
        {
            Masa = masa;
        }

        return Page();
    }
}
