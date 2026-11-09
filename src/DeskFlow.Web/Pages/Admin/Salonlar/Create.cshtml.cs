using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeskFlow.Web.Pages.Admin.Salonlar;

[Authorize(Roles = Roller.Yonetici)]
public class CreateModel : PageModel
{
    private readonly AppDbContext _db;

    public CreateModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Salon Salon { get; set; } = new()
    {
        Kapasite = 50,
        AcilisSaati = new TimeOnly(8, 0),
        KapanisSaati = new TimeOnly(22, 0)
    };

    public void OnGet()
    {
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

        _db.Salonlar.Add(new Salon
        {
            Ad = Salon.Ad.Trim(),
            Kat = Salon.Kat,
            Kapasite = Salon.Kapasite,
            AcilisSaati = Salon.AcilisSaati,
            KapanisSaati = Salon.KapanisSaati,
            AktifMi = Salon.AktifMi
        });
        await _db.SaveChangesAsync();

        TempData["Basari"] = $"“{Salon.Ad.Trim()}” salonu eklendi.";
        return RedirectToPage("Index");
    }
}
