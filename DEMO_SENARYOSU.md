# DeskFlow Web – Demo Senaryosu (taslak)

Hesaplar: **Yönetici** `yonetici@deskflow.local` (şifre: user-secrets'taki `VarsayilanYonetici:Sifre`), **Öğrenci**: video öncesi `/KayitOl` ile oluşturulacak deneme hesabı.

1. **Giriş yapmadan** ana sayfa ve Salonlar sayfası → herkesin gördüğü ekran. `/Admin` adresine gidilince giriş sayfasına yönlendirildiği gösterilir.
2. **Öğrenci kaydı** → `/KayitOl` ile hesap açılır, giriş yapılır. Menüde "Salon Yönetimi" ve "Yönetim" linklerinin görünmediği, `/Admin` adresinde "yetkisiz erişim" sayfasının çıktığı gösterilir.
3. **Yönetici girişi** → çıkış yapılıp yönetici ile girilir. Yönetim panelindeki sayılar (salon, masa, öğrenci) gösterilir.
4. **Salon CRUD** → yeni salon ekle, kapanış saatini açılıştan önce yazıp doğrulama hatasını göster, düzenle, sil (silme sayfasındaki "N masa da silinecek" uyarısı).
5. **Masa CRUD** → yeni masa ekle (aynı salonda aynı numara hatası), düzenle, sil.
6. **Swagger** → aynı tarayıcıda `http://localhost:5080/swagger`:
   - `GET /api/masalar?salonId=1&prizli=true` → filtreli liste (giriş gerekmez).
   - `POST /api/masalar` → 201 Created.
   - Aynı salonda aynı masa numarasıyla tekrar POST → 400.
   - Çıkış yapıp (ya da gizli pencerede) `POST` → 401; öğrenci ile `POST` → 403.
   - Rezervasyonu olan masada `DELETE` → 409.
7. **Veritabanı** → pgAdmin'de `deskflow` veritabanındaki tabloları göster, user-secrets'ta şifrenin neden durduğunu anlat.
