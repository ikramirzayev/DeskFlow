using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Api.Models;

public class Masa
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Masa numarası zorunludur.")]
    [StringLength(20)]
    public string MasaNo { get; set; } = string.Empty;

    // Zorunlu yabancı anahtar (int, nullable değil) => her masanın bir salonu olmak zorunda
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
}
