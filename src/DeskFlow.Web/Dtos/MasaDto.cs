namespace DeskFlow.Web.Dtos;

/// <summary>Masa listeleme / detay cevabı. Salon ve Rezervasyonlar alanları dönmez (JSON döngüsünü önler).</summary>
public class MasaDto
{
    public int Id { get; set; }
    public string MasaNo { get; set; } = string.Empty;
    public int SalonId { get; set; }
    public string SalonAd { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public bool PrizliMi { get; set; }
    public bool SessizBolgeMi { get; set; }
    public bool AktifMi { get; set; }
}
