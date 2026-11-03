using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DeskFlow.Web.Models;
using DeskFlow.Web.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DeskFlow.Web.Pages.Admin.Masalar;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet()
    {
        ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");
        return Page();
    }

    [BindProperty]
    public Masa Masa { get; set; } = default!;

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Masa.Salon");

        if (string.IsNullOrWhiteSpace(Masa.QrKod))
        {
            Masa.QrKod = $"DESKFLOW-S{Masa.SalonId}-{Masa.MasaNo}";
            ModelState.Remove("Masa.QrKod");
        }
        bool varMi = await _context.Masalar.AnyAsync(m => m.SalonId == Masa.SalonId && m.MasaNo == Masa.MasaNo);
        if (varMi)
        {
            ModelState.AddModelError("Masa.MasaNo", $"Bu salonda {Masa.MasaNo} numaralı masa zaten var.");
        }
        if (!ModelState.IsValid)
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");
            return Page();
        }

        _context.Masalar.Add(Masa);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
