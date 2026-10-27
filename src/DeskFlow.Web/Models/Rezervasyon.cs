using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Web.Models;

public class Rezervasyon
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;

    [Required]
    public int MasaId { get; set; }

    [Required(ErrorMessage = "Başlangıç zamanı zorunludur.")]
    public DateTime Baslangic { get; set; }

    [Required(ErrorMessage = "Bitiş zamanı zorunludur.")]
    public DateTime Bitis { get; set; }

    public RezervasyonDurumu Durum { get; set; } = RezervasyonDurumu.Beklemede;

    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

    public Kullanici Kullanici { get; set; } = null!;

    public Masa Masa { get; set; } = null!;

    public List<CheckInKaydi> CheckInKayitlari { get; set; } = new();
}
