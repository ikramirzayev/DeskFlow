using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Web.Dtos;

/// <summary>Masa ekleme ve güncelleme isteği.</summary>
public class MasaKaydetDto
{
    [Required(ErrorMessage = "Masa numarası zorunludur.")]
    [StringLength(20, ErrorMessage = "Masa numarası en fazla 20 karakter olabilir.")]
    public string MasaNo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir salon seçilmelidir.")]
    public int SalonId { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public bool PrizliMi { get; set; }

    public bool SessizBolgeMi { get; set; }

    public bool AktifMi { get; set; } = true;
}
