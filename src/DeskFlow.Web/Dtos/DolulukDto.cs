namespace DeskFlow.Web.Dtos;

/// <summary>Bir salonun belirli bir gündeki doluluğu: her masanın dolu saat aralıkları.</summary>
public class DolulukDto
{
    public int SalonId { get; set; }
    public string SalonAd { get; set; } = string.Empty;
    public DateOnly Tarih { get; set; }
    public List<MasaDolulukDto> Masalar { get; set; } = new();
}

public class MasaDolulukDto
{
    public int MasaId { get; set; }
    public string MasaNo { get; set; } = string.Empty;
    public List<DoluAralikDto> DoluAraliklar { get; set; } = new();
}

public class DoluAralikDto
{
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
}
