using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Services;

public enum CheckInDurumu
{
    Tamam,
    MasaBulunamadi,
    RezervasyonYok,
    ErkenGeldi,
    GecKaldi
}

public class CheckInSonucu
{
    public CheckInDurumu Durum { get; private init; }
    public Rezervasyon? Rezervasyon { get; private init; }

    public bool Basarili => Durum == CheckInDurumu.Tamam;

    public static CheckInSonucu Tamam(Rezervasyon rezervasyon) =>
        new() { Durum = CheckInDurumu.Tamam, Rezervasyon = rezervasyon };

    public static CheckInSonucu Hatali(CheckInDurumu durum) =>
        new() { Durum = durum };

    public string Mesaj => Durum switch
    {
        CheckInDurumu.Tamam => "Check-in yapıldı. İyi çalışmalar!",
        CheckInDurumu.MasaBulunamadi => "Bu QR koduna ait bir masa bulunamadı.",
        CheckInDurumu.RezervasyonYok => "Bu masada size ait bekleyen bir rezervasyon yok.",
        CheckInDurumu.ErkenGeldi => $"Henüz erken. Check-in rezervasyon başlangıcından en fazla {CheckInServisi.OnceDakika} dakika önce yapılabilir.",
        CheckInDurumu.GecKaldi => $"Geç kaldınız. Check-in rezervasyon başlangıcından en fazla {CheckInServisi.GecikmeDakika} dakika sonrasına kadar yapılabilir.",
        _ => string.Empty
    };
}

/// QR kod (masanın QrKod değeri) ile check-in yapar.
public class CheckInServisi
{
    public const int OnceDakika = 15;
    public const int GecikmeDakika = 15;

    private readonly AppDbContext _db;

    public CheckInServisi(AppDbContext db)
    {
        _db = db;
    }

    public Task<CheckInSonucu> CheckInYapAsync(string kullaniciId, string qrKod) =>
        CheckInYapAsync(kullaniciId, qrKod, DateTime.UtcNow);

    /// Zaman dışarıdan verilebilir (test için). Veritabanındaki zamanlar UTC olduğu için simdiUtc UTC olmalı.
    public async Task<CheckInSonucu> CheckInYapAsync(string kullaniciId, string qrKod, DateTime simdiUtc)
    {
        var kod = (qrKod ?? string.Empty).Trim();

        var masa = string.IsNullOrEmpty(kod)
            ? null
            : await _db.Masalar.AsNoTracking()
                .FirstOrDefaultAsync(m => m.QrKod.ToLower() == kod.ToLower());

        if (masa is null)
        {
            return CheckInSonucu.Hatali(CheckInDurumu.MasaBulunamadi);
        }

        var bekleyenler = await _db.Rezervasyonlar
            .Where(r => r.KullaniciId == kullaniciId &&
                        r.MasaId == masa.Id &&
                        r.Durum == RezervasyonDurumu.Beklemede)
            .OrderBy(r => r.Baslangic)
            .ToListAsync();

        if (bekleyenler.Count == 0)
        {
            return CheckInSonucu.Hatali(CheckInDurumu.RezervasyonYok);
        }

        var pencerede = bekleyenler.FirstOrDefault(r =>
            simdiUtc >= r.Baslangic.AddMinutes(-OnceDakika) &&
            simdiUtc <= r.Baslangic.AddMinutes(GecikmeDakika));

        if (pencerede is null)
        {
            // Henüz başlamamış bir rezervasyon varsa "erken", yoksa hepsi geçmişte kalmıştır.
            var erken = bekleyenler.Any(r => r.Baslangic.AddMinutes(-OnceDakika) > simdiUtc);
            return CheckInSonucu.Hatali(erken ? CheckInDurumu.ErkenGeldi : CheckInDurumu.GecKaldi);
        }

        pencerede.Durum = RezervasyonDurumu.Aktif;
        _db.CheckInKayitlari.Add(new CheckInKaydi
        {
            RezervasyonId = pencerede.Id,
            CheckInZamani = simdiUtc,
            IslemTuru = "CheckIn"
        });
        await _db.SaveChangesAsync();

        return CheckInSonucu.Tamam(pencerede);
    }
}
