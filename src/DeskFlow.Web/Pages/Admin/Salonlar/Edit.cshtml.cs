using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages.Admin.Salonlar;

[Authorize(Roles = Roller.Yonetici)]
public class EditModel : PageModel
{
    private readonly AppDbContext _db;

    public EditModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Salon Salon { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var salon = await _db.Salonlar.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (salon is null)
        {
            return NotFound();
        }

        Salon = salon;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Salon.Masalar");

        if (Salon.KapanisSaati <= Salon.AcilisSaati)
        {
            ModelState.AddModelError("Salon.KapanisSaati", "Kapanış saati açılış saatinden sonra olmalıdır.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var salon = await _db.Salonlar.FirstOrDefaultAsync(s => s.Id == Salon.Id);
        if (salon is null)
        {
            return NotFound();
        }

        salon.Ad = Salon.Ad.Trim();
        salon.Kat = Salon.Kat;
        salon.Kapasite = Salon.Kapasite;
        salon.AcilisSaati = Salon.AcilisSaati;
        salon.KapanisSaati = Salon.KapanisSaati;
        salon.AktifMi = Salon.AktifMi;

        await _db.SaveChangesAsync();

        TempData["Basari"] = $"“{salon.Ad}” salonu güncellendi.";
        return RedirectToPage("Index");
    }
}
