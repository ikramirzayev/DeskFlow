using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages.Admin.Salonlar;

[Authorize(Roles = Roller.Yonetici)]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<SalonSatiri> Salonlar { get; set; } = new();

    public class SalonSatiri
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int Kat { get; set; }
        public int Kapasite { get; set; }
        public TimeOnly AcilisSaati { get; set; }
        public TimeOnly KapanisSaati { get; set; }
        public bool AktifMi { get; set; }
        public int MasaSayisi { get; set; }
    }

    public async Task OnGetAsync()
    {
        Salonlar = await _db.Salonlar
            .AsNoTracking()
            .OrderBy(s => s.Kat)
            .ThenBy(s => s.Ad)
            .Select(s => new SalonSatiri
            {
                Id = s.Id,
                Ad = s.Ad,
                Kat = s.Kat,
                Kapasite = s.Kapasite,
                AcilisSaati = s.AcilisSaati,
                KapanisSaati = s.KapanisSaati,
                AktifMi = s.AktifMi,
                MasaSayisi = s.Masalar.Count
            })
            .ToListAsync();
    }
}
