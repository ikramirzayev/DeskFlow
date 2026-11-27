using System.Security.Claims;
using DeskFlow.Web.Dtos;
using DeskFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskFlow.Web.Controllers;

/// CheckInServisi'ni Swagger'dan denemek için küçük bir uç nokta.
[ApiController]
[Route("api/checkin")]
[Authorize]
public class CheckInController : ControllerBase
{
    private readonly CheckInServisi _servis;

    public CheckInController(CheckInServisi servis)
    {
        _servis = servis;
    }

    // POST /api/checkin  { "qrKod": "DF-S1-M03" }
    // 200 başarılı, 404 masa yok, 409 rezervasyon yok / erken / geç
    [HttpPost]
    public async Task<IActionResult> CheckIn(CheckInIstekDto dto)
    {
        var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sonuc = await _servis.CheckInYapAsync(kullaniciId, dto.QrKod);

        var govde = new { durum = sonuc.Durum.ToString(), mesaj = sonuc.Mesaj };

        return sonuc.Durum switch
        {
            CheckInDurumu.Tamam => Ok(govde),
            CheckInDurumu.MasaBulunamadi => NotFound(govde),
            _ => Conflict(govde)
        };
    }
}
