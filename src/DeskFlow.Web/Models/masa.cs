using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Web.Models;

public class Masa
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Masa numarası zorunludur.")]
    [StringLength(20)]
    public string MasaNo { get; set; } = string.Empty;

    [Required]
    public int SalonId { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public bool PrizliMi { get; set; }

    public bool SessizBolgeMi { get; set; }

    [Required(ErrorMessage = "QR kod zorunludur.")]
    [StringLength(200)]
    public string QrKod { get; set; } = string.Empty;

    public bool AktifMi { get; set; } = true;

    public Salon Salon { get; set; } = null!;

    public List<Rezervasyon> Rezervasyonlar { get; set; } = new();
}