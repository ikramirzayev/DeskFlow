# DeskFlow

**Akıllı Çalışma Alanı ve Kütüphane Masa Rezervasyon Sistemi**

DeskFlow, üniversite kütüphanesi ve çalışma salonlarındaki masaların anlık doluluğunu kat planı üzerinde gösteren, masaların önceden rezerve edilmesini ve masadaki QR kod okutularak check-in yapılmasını sağlayan bir web portalıdır.

> BLM4581 Ağ Tabanlı Teknolojiler ve Uygulamaları – 2026–2027 Güz dönem projesi

## Ekip

| Ad Soyad | GitHub |
|---|---|
| İkram İRZAYEV | [@ikramirzayev](https://github.com/ikramirzayev) |
| Ali KARA | [@Ali-kara-hub](https://github.com/Ali-kara-hub) |
| Zeynep Şevval YILDIZ | [@zyldzz](https://github.com/zyldzz) |

## Teknolojiler

- ASP.NET Core Razor Pages (.NET 10)
- Entity Framework Core (Code First, migration)
- PostgreSQL (Npgsql)
- ASP.NET Core Identity (Yönetici ve Öğrenci rolleri)
- Web API + Swagger
- Bootstrap 5

## Proje yapısı

```
DeskFlow/
├── DeskFlow.sln
└── src/
    └── DeskFlow.Web/        # Razor Pages uygulaması (sayfalar, API, veri erişimi)
```

## Kurulum

### Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (16 veya üzeri)
- EF Core komut satırı aracı: `dotnet tool install --global dotnet-ef`

### 1. Depoyu klonlayın

```bash
git clone https://github.com/ikramirzayev/DeskFlow.git
cd DeskFlow
```

### 2. Veritabanı bağlantısını ayarlayın

Veritabanı şifresi **depoya konmaz**. `appsettings.json` içindeki `DefaultConnection` boş bırakılmıştır; her geliştirici kendi bağlantı dizesini [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) ile kendi bilgisayarında tutar:

```bash
cd src/DeskFlow.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=deskflow;Username=postgres;Password=SIFRENIZ"
dotnet user-secrets set "VarsayilanYonetici:Sifre" "yonetici1"
```

Uygulama ilk açıldığında `Yonetici` ve `Ogrenci` rolleri ile `yonetici@deskflow.local` adresli bir yönetici hesabı otomatik oluşturulur. Yönetici şifresi user-secrets'tan okunur (en az 6 karakter, bir rakam ve bir küçük harf içermeli).

### 3. Veritabanını oluşturun (migration)

```bash
dotnet ef database update
```

### 4. Uygulamayı çalıştırın

```bash
dotnet run
```

Uygulama `https://localhost:7176` (veya `http://localhost:5137`) adresinde açılır.

## Çalışma kuralları

- Herkes kendi GitHub hesabıyla commit ve push yapar.
- Her hafta en az bir kez push yapılır.
- Push yapmadan önce `git pull` ile güncel kodu alın.
- Şifre, bağlantı dizesi, API anahtarı gibi bilgiler asla commit edilmez.
- Commit mesajları kısa ve açıklayıcı olur (ör. `Salon CRUD sayfaları eklendi`).
