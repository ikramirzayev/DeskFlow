namespace DeskFlow.Web.Models;

/// Durumun ekranda görünen Türkçe adı ve rozet rengi tek yerde;
/// Rezervasyonlarım ve Admin/Rezervasyonlar aynı metni kullanır.
public static class RezervasyonDurumuUzantilari
{
    public static string Metin(this RezervasyonDurumu durum) => durum switch
    {
        RezervasyonDurumu.Beklemede => "Beklemede",
        RezervasyonDurumu.Aktif => "Aktif",
        RezervasyonDurumu.Tamamlandi => "Tamamlandı",
        RezervasyonDurumu.Iptal => "İptal edildi",
        RezervasyonDurumu.Gelmedi => "Gelinmedi",
        _ => durum.ToString()
    };

    public static string RozetRengi(this RezervasyonDurumu durum) => durum switch
    {
        RezervasyonDurumu.Beklemede => "text-bg-warning",
        RezervasyonDurumu.Aktif => "text-bg-success",
        RezervasyonDurumu.Tamamlandi => "text-bg-secondary",
        RezervasyonDurumu.Iptal => "text-bg-danger",
        _ => "text-bg-dark"
    };
}
