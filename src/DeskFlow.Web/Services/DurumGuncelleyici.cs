using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Services;

/// Arka planda her dakika rezervasyon durumlarını günceller:
///  - Başlangıçtan 15 dk geçmiş ve check-in yapılmamış (Beklemede) -> Gelmedi
///  - Bitişi geçmiş Aktif rezervasyon -> Tamamlandi
public class DurumGuncelleyici : BackgroundService
{
    private static readonly TimeSpan Aralik = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DurumGuncelleyici> _logger;

    public DurumGuncelleyici(IServiceScopeFactory scopeFactory, ILogger<DurumGuncelleyici> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var zamanlayici = new PeriodicTimer(Aralik);

        do
        {
            try
            {
                // AppDbContext scoped olduğu için her turda yeni scope açılır.
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await GuncelleAsync(db, DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Veritabanı hazır değilse vb. uygulama çökmesin, bir sonraki turda tekrar denensin.
                _logger.LogWarning(ex, "Rezervasyon durumları güncellenemedi.");
            }
        }
        while (await SonrakiTurAsync(zamanlayici, stoppingToken));
    }

    private static async Task<bool> SonrakiTurAsync(PeriodicTimer zamanlayici, CancellationToken ct)
    {
        try
        {
            return await zamanlayici.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// Kurallar tek yerde; test için doğrudan da çağrılabilir. Güncellenen kayıt sayısını döner.
    public static async Task<int> GuncelleAsync(AppDbContext db, DateTime simdiUtc, CancellationToken ct = default)
    {
        var gelmedi = await db.Rezervasyonlar
            .Where(r => r.Durum == RezervasyonDurumu.Beklemede &&
                        r.Baslangic < simdiUtc.AddMinutes(-CheckInServisi.GecikmeDakika))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Durum, RezervasyonDurumu.Gelmedi), ct);

        var tamamlandi = await db.Rezervasyonlar
            .Where(r => r.Durum == RezervasyonDurumu.Aktif && r.Bitis < simdiUtc)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Durum, RezervasyonDurumu.Tamamlandi), ct);

        return gelmedi + tamamlandi;
    }
}
