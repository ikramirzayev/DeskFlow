using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Api.Models;

public class Salon
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Salon adı zorunludur.")]
    [StringLength(100)]
    public string Ad { get; set; } = string.Empty;

    [Range(-2, 20, ErrorMessage = "Kat -2 ile 20 arasında olmalıdır.")]
    public int Kat { get; set; }

    [Range(1, 1000, ErrorMessage = "Kapasite 1 ile 1000 arasında olmalıdır.")]
    public int Kapasite { get; set; }

    public TimeOnly AcilisSaati { get; set; }

    public TimeOnly KapanisSaati { get; set; }

    public bool AktifMi { get; set; } = true;

    // Bire-çok: bir salonda birden çok masa
    public List<Masa> Masalar { get; set; } = new();
}
