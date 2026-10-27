using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Data;

public class AppDbContext : IdentityDbContext<Kullanici>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Salon> Salonlar { get; set; }
    public DbSet<Masa> Masalar { get; set; }
    public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
    public DbSet<CheckInKaydi> CheckInKayitlari { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tablolarının (AspNetUsers, AspNetRoles...) ayarları için ilk satır şart.
        base.OnModelCreating(builder);

        // Salon silinince masaları da silinsin.
        builder.Entity<Salon>()
            .HasMany(s => s.Masalar)
            .WithOne(m => m.Salon)
            .HasForeignKey(m => m.SalonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Rezervasyon kaydı olan masa veya kullanıcı yanlışlıkla silinemesin.
        builder.Entity<Masa>()
            .HasMany(m => m.Rezervasyonlar)
            .WithOne(r => r.Masa)
            .HasForeignKey(r => r.MasaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Kullanici>()
            .HasMany(k => k.Rezervasyonlar)
            .WithOne(r => r.Kullanici)
            .HasForeignKey(r => r.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        // Rezervasyon silinince check-in kayıtları da silinsin.
        builder.Entity<Rezervasyon>()
            .HasMany(r => r.CheckInKayitlari)
            .WithOne(c => c.Rezervasyon)
            .HasForeignKey(c => c.RezervasyonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Durum veritabanında okunabilir metin olarak saklansın (Aktif, Iptal...).
        builder.Entity<Rezervasyon>()
            .Property(r => r.Durum)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Aynı salonda aynı masa numarası ve aynı QR kod iki kez olmasın.
        builder.Entity<Masa>()
            .HasIndex(m => new { m.SalonId, m.MasaNo })
            .IsUnique();
        builder.Entity<Masa>()
            .HasIndex(m => m.QrKod)
            .IsUnique();

        // Örnek veri: 2 salon ve 10 masa.
        builder.Entity<Salon>().HasData(
            new { Id = 1, Ad = "Merkez Kütüphane - Sessiz Salon", Kat = 1, Kapasite = 60,
                  AcilisSaati = new TimeOnly(8, 0), KapanisSaati = new TimeOnly(23, 0), AktifMi = true },
            new { Id = 2, Ad = "Grup Çalışma Salonu", Kat = 2, Kapasite = 40,
                  AcilisSaati = new TimeOnly(9, 0), KapanisSaati = new TimeOnly(22, 0), AktifMi = true });

        builder.Entity<Masa>().HasData(
            new { Id = 1, MasaNo = "A1", SalonId = 1, X = 1.0, Y = 1.0, PrizliMi = true,  SessizBolgeMi = true,  QrKod = "DESKFLOW-S1-A1", AktifMi = true },
            new { Id = 2, MasaNo = "A2", SalonId = 1, X = 3.0, Y = 1.0, PrizliMi = true,  SessizBolgeMi = true,  QrKod = "DESKFLOW-S1-A2", AktifMi = true },
            new { Id = 3, MasaNo = "A3", SalonId = 1, X = 5.0, Y = 1.0, PrizliMi = false, SessizBolgeMi = true,  QrKod = "DESKFLOW-S1-A3", AktifMi = true },
            new { Id = 4, MasaNo = "A4", SalonId = 1, X = 1.0, Y = 3.0, PrizliMi = false, SessizBolgeMi = true,  QrKod = "DESKFLOW-S1-A4", AktifMi = true },
            new { Id = 5, MasaNo = "A5", SalonId = 1, X = 3.0, Y = 3.0, PrizliMi = true,  SessizBolgeMi = false, QrKod = "DESKFLOW-S1-A5", AktifMi = true },
            new { Id = 6, MasaNo = "A6", SalonId = 1, X = 5.0, Y = 3.0, PrizliMi = false, SessizBolgeMi = false, QrKod = "DESKFLOW-S1-A6", AktifMi = true },
            new { Id = 7, MasaNo = "B1", SalonId = 2, X = 1.0, Y = 1.0, PrizliMi = true,  SessizBolgeMi = false, QrKod = "DESKFLOW-S2-B1", AktifMi = true },
            new { Id = 8, MasaNo = "B2", SalonId = 2, X = 3.0, Y = 1.0, PrizliMi = true,  SessizBolgeMi = false, QrKod = "DESKFLOW-S2-B2", AktifMi = true },
            new { Id = 9, MasaNo = "B3", SalonId = 2, X = 1.0, Y = 3.0, PrizliMi = false, SessizBolgeMi = false, QrKod = "DESKFLOW-S2-B3", AktifMi = true },
            new { Id = 10, MasaNo = "B4", SalonId = 2, X = 3.0, Y = 3.0, PrizliMi = false, SessizBolgeMi = false, QrKod = "DESKFLOW-S2-B4", AktifMi = true });
    }
}
