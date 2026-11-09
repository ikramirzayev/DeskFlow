using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Services;

public class RezervasyonSonucu
{
    public bool Basarili { get; private init; }
    public string? Hata { get; private init; }
    public Rezervasyon? Rezervasyon { get; private init; }

    public static RezervasyonSonucu Tamam(Rezervasyon rezervasyon) =>
        new() { Basarili = true, Rezervasyon = rezervasyon };

    public static RezervasyonSonucu Hatali(string hata) =>
        new() { Basarili = false, Hata = hata };
}

/// Rezervasyon kuralları tek yerde: hem web sayfası hem ileride API bunu kullanır.
public class RezervasyonServisi
{
    public const int EnAzDakika = 30;
    public const int EnFazlaSaat = 4;
    public const int EnFazlaGunSonra = 7;

    private readonly AppDbContext _db;

    public RezervasyonServisi(AppDbContext db)
    {
        _db = db;
    }

    /// Zamanlar kullanıcının yerel saatiyle gelir, veritabanına UTC olarak yazılır.
    public async Task<RezervasyonSonucu> OlusturAsync(
        string kullaniciId, int masaId, DateTime baslangic, DateTime bitis)
    {
        var masa = await _db.Masalar
            .Include(m => m.Salon)
            .FirstOrDefaultAsync(m => m.Id == masaId);

        if (masa is null)
        {
            return RezervasyonSonucu.Hatali("Masa bulunamadı.");
        }

        var kuralHatasi = KurallariKontrolEt(masa, baslangic, bitis, DateTime.Now);
        if (kuralHatasi is not null)
        {
            return RezervasyonSonucu.Hatali(kuralHatasi);
        }

        var baslangicUtc = YereldenUtc(baslangic);
        var bitisUtc = YereldenUtc(bitis);

        // İki zaman aralığı çakışır: biri bitmeden öbürü başlıyorsa.
        var masaDolu = await _db.Rezervasyonlar.AnyAsync(r =>
            r.MasaId == masaId &&
            (r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif) &&
            r.Baslangic < bitisUtc && baslangicUtc < r.Bitis);

        if (masaDolu)
        {
            return RezervasyonSonucu.Hatali("Bu masa seçilen saatte dolu. Lütfen başka bir saat ya da masa seçin.");
        }

        var kullaniciMesgul = await _db.Rezervasyonlar.AnyAsync(r =>
            r.KullaniciId == kullaniciId &&
            (r.Durum == RezervasyonDurumu.Beklemede || r.Durum == RezervasyonDurumu.Aktif) &&
            r.Baslangic < bitisUtc && baslangicUtc < r.Bitis);

        if (kullaniciMesgul)
        {
            return RezervasyonSonucu.Hatali("Bu saatte zaten bir rezervasyonunuz var.");
        }

        var rezervasyon = new Rezervasyon
        {
            KullaniciId = kullaniciId,
            MasaId = masaId,
            Baslangic = baslangicUtc,
            Bitis = bitisUtc,
            Durum = RezervasyonDurumu.Beklemede,
            OlusturmaTarihi = DateTime.UtcNow
        };

        _db.Rezervasyonlar.Add(rezervasyon);
        await _db.SaveChangesAsync();

        return RezervasyonSonucu.Tamam(rezervasyon);
    }

    /// Veritabanına gitmeden kontrol edilebilen kurallar. Hata yoksa null döner.
    public static string? KurallariKontrolEt(Masa masa, DateTime baslangic, DateTime bitis, DateTime simdi)
    {
        if (!masa.AktifMi || !masa.Salon.AktifMi)
        {
            return "Bu masa şu anda rezervasyona kapalı.";
        }

        if (bitis <= baslangic)
        {
            return "Bitiş saati başlangıç saatinden sonra olmalıdır.";
        }

        if (baslangic < simdi)
        {
            return "Geçmiş bir zamana rezervasyon yapılamaz.";
        }

        if (baslangic.Date > simdi.Date.AddDays(EnFazlaGunSonra))
        {
            return $"En fazla {EnFazlaGunSonra} gün sonrasına rezervasyon yapılabilir.";
        }

        if (baslangic.Date != bitis.Date)
        {
            return "Rezervasyon aynı gün içinde başlayıp bitmelidir.";
        }

        var sure = bitis - baslangic;
        if (sure < TimeSpan.FromMinutes(EnAzDakika))
        {
            return $"Rezervasyon en az {EnAzDakika} dakika olmalıdır.";
        }

        if (sure > TimeSpan.FromHours(EnFazlaSaat))
        {
            return $"Rezervasyon en fazla {EnFazlaSaat} saat olabilir.";
        }

        var acilis = masa.Salon.AcilisSaati;
        var kapanis = masa.Salon.KapanisSaati;
        if (TimeOnly.FromDateTime(baslangic) < acilis || TimeOnly.FromDateTime(bitis) > kapanis)
        {
            return $"Salon {acilis:HH\\:mm} – {kapanis:HH\\:mm} saatleri arasında açıktır.";
        }

        return null;
    }

    public static DateTime YereldenUtc(DateTime yerel) =>
        DateTime.SpecifyKind(yerel, DateTimeKind.Local).ToUniversalTime();
}
