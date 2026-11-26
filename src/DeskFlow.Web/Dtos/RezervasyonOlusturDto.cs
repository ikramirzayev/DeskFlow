using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Web.Dtos;

/// <summary>Rezervasyon oluşturma isteği. Saatler yerel saatle ve "Z" olmadan gönderilir (örn. 2026-11-27T10:00:00).</summary>
public class RezervasyonOlusturDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir masa seçilmelidir.")]
    public int MasaId { get; set; }

    [Required(ErrorMessage = "Başlangıç zamanı zorunludur.")]
    public DateTime? Baslangic { get; set; }

    [Required(ErrorMessage = "Bitiş zamanı zorunludur.")]
    public DateTime? Bitis { get; set; }
}
