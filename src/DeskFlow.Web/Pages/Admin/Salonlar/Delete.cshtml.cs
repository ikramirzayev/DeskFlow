using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages.Admin.Salonlar;

[Authorize(Roles = Roller.Yonetici)]
public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;

    public DeleteModel(AppDbContext db)
    {
        _db = db;
    }

    public Salon Salon { get; set; } = new();

    public int MasaSayisi { get; set; }

    // Bu salonun masalarına ait rezervasyon sayısı. Varsa salon silinemez.
    public int RezervasyonSayisi { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        return await YukleAsync(id.Value) ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        if (!await YukleAsync(id.Value))
        {
            return NotFound();
        }

        if (RezervasyonSayisi > 0)
        {
            TempData["Hata"] = "Bu salonun masalarına ait rezervasyonlar olduğu için salon silinemedi.";
            return RedirectToPage("Index");
        }

        var salon = await _db.Salonlar.FindAsync(id.Value);
        if (salon is null)
        {
            return NotFound();
        }

        try
        {
            _db.Salonlar.Remove(salon);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Hata"] = "Salon silinirken bir hata oluştu. Salona bağlı kayıtlar olabilir.";
            return RedirectToPage("Index");
        }

        TempData["Basari"] = $"“{salon.Ad}” salonu ve {MasaSayisi} masası silindi.";
        return RedirectToPage("Index");
    }

    private async Task<bool> YukleAsync(int id)
    {
        var salon = await _db.Salonlar.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (salon is null)
        {
            return false;
        }

        Salon = salon;
        MasaSayisi = await _db.Masalar.CountAsync(m => m.SalonId == id);
        RezervasyonSayisi = await _db.Rezervasyonlar.CountAsync(r => r.Masa.SalonId == id);
        return true;
    }
}
