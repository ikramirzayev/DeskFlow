namespace DeskFlow.Web.Dtos;

/// <summary>Rezervasyon cevabı. Saatler sunucunun yerel saatiyle döner.</summary>
public class RezervasyonDto
{
    public int Id { get; set; }
    public int MasaId { get; set; }
    public string MasaNo { get; set; } = string.Empty;
    public int SalonId { get; set; }
    public string SalonAd { get; set; } = string.Empty;
    public int Kat { get; set; }
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public string Durum { get; set; } = string.Empty;
    public bool IptalEdilebilir { get; set; }
}
