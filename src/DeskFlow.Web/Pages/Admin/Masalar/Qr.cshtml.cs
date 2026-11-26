using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace DeskFlow.Web.Pages.Admin.Masalar;

/// Masaya yapıştırılacak QR çıktısı. QR'ın içinde sadece masanın QrKod değeri vardır;
/// mobil uygulama bunu okuyup /api/checkin'e gönderir.
public class QrModel : PageModel
{
    private readonly AppDbContext _db;

    public QrModel(AppDbContext db)
    {
        _db = db;
    }

    public Masa Masa { get; set; } = default!;

    // PNG resmi base64 metni olarak tutulur; sayfada img etiketinin src'sine
    // "data:image/png;base64,..." diye yazılır, diske dosya kaydetmeye gerek kalmaz.
    public string QrResmiBase64 { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var masa = await _db.Masalar
            .AsNoTracking()
            .Include(m => m.Salon)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (masa is null)
        {
            return NotFound();
        }

        Masa = masa;
        QrResmiBase64 = Convert.ToBase64String(QrPngUret(masa.QrKod));
        return Page();
    }

    public static byte[] QrPngUret(string metin)
    {
        using var uretici = new QRCodeGenerator();
        // Q seviyesi: kodun yaklaşık %25'i çizilse/kirlense de okunur; masaya yapıştırılan kâğıt için uygun.
        using var veri = uretici.CreateQrCode(metin, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(veri).GetGraphic(20);
    }
}
