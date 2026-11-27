using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Web.Dtos;

public class CheckInIstekDto
{
    [Required(ErrorMessage = "QR kod zorunludur.")]
    [StringLength(200)]
    public string QrKod { get; set; } = string.Empty;
}
