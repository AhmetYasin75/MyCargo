🚚 MyCargo — Kargo Takip Sistemi
ASP.NET Core 8 MVC ile geliştirilmiş, N-Layer mimarili, rol tabanlı uçtan uca bir kargo takip uygulaması.
Müşteri kargo gönderir, şube personeli durumu ilerletir, yönetici raporları izler; herkes takip numarasıyla
kargosunu giriş yapmadan sorgulayabilir.
> MyAcademy eğitimi kapsamında geliştirilen **CargoTrack** vaka çalışmasının tamamlanmış hâlidir.

---
✨ Özellikler
Herkese açık
Takip numarasıyla kargo sorgulama (`MYC-2026-xxxxxx`), giriş gerekmez
Durum, ilerleme çubuğu, tahmini teslimat ve hareket geçmişi (zaman çizelgesi)
Kişisel veri göstermez: gönderici/alıcı adı, teslimat kodu ve fiyat herkese açık sayfada yoktur
Müşteri (User)
Kargo oluşturma; ücret desi, bölge ve teslimat tipine göre otomatik hesaplanır
Kişisel panel: yoldaki/geçmiş kargolar, son kargonun zaman çizelgesi
Teslimat kodu yalnızca alıcıya ve yalnızca kargo "Dağıtıma Çıktı" iken görünür
Şube Müdürü (Manager)
Şube paneli: bugün gelen/çıkan, dağıtımdaki, geciken ve sorunlu kargolar
Kargo durumunu ilerletme, teslimat kodunu doğrulama, başarısız teslimatı bildirme (yalnızca kendi şubesi)
Yönetici (Admin)
Yönetim paneli ve raporlar: şube/personel performansı, ortalama teslimat süresi, zamanında teslimat ve iade oranı
Şube, aktarma merkezi, personel, fiyat kuralı ve rol yönetimi
Değiştirilemez Audit Log (oluşturma, güncelleme, silme, durum değişikliği)
---
📐 İş Kuralları
Durum akışı: `Oluşturuldu → Gönderici Şubesinde → Transfer Merkezinde → Varış Şubesinde → Dağıtıma Çıktı → Teslim Edildi`. Kurallara aykırı geçiş reddedilir.
Teslim: Yalnızca alıcıdaki 6 haneli kod doğrulanırsa tamamlanır.
İade: 3 başarısız teslimat denemesinde kargo otomatik İade Sürecine alınır.
Fiyat: `taban fiyat + max(desi, ağırlık) × birim fiyat + sigorta`, desi = `en × boy × yükseklik / 3000`. Fiyat kargo üzerinde saklanır, sonradan kural değişse eski kargo etkilenmez.
Silme: Soft delete. Kayıtlar veritabanından fiziksel olarak silinmez.
Yetki: Kurallar yalnızca controller'da değil, servis katmanında da kontrol edilir.
---
🏗️ Mimari
```
CargoTrack.Entity      → Varlıklar, enum'lar
CargoTrack.DTO         → Veri taşıma nesneleri
CargoTrack.DataAccess  → DbContext, repository'ler, migration'lar
CargoTrack.Business    → İş kuralları, servisler, doğrulayıcılar (FluentValidation)
CargoTrack.WebUI       → MVC (controller, view), Areas (Admin, Manager)
```
Alan	Teknoloji
Framework	.NET 8, ASP.NET Core MVC
Veri	Entity Framework Core, SQL Server, Lazy Loading Proxies
Kimlik	ASP.NET Core Identity (Guid), rol tabanlı yetkilendirme
Doğrulama	FluentValidation (Türkçe mesajlar)
Eşleme	Mapster (yalnızca Business katmanında)
Arayüz	Bootstrap + özel tema, herkese açık sayfalarda Tailwind CSS
Diğer	Global exception filter, audit log, sayfalama/arama/filtreleme
