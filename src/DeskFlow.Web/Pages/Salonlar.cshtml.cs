using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Pages
{
    public class SalonlarModel : PageModel
    {
        private readonly AppDbContext _db;

        public SalonlarModel(AppDbContext db)
        {
            _db = db;
        }

        public List<Salon> Salonlar { get; set; } = new();

        public async Task OnGetAsync()
        {
            Salonlar = await _db.Salonlar
                .Where(s => s.AktifMi)
                .Include(s => s.Masalar)
                .OrderBy(s => s.Ad)
                .ToListAsync();
        }
    }
}