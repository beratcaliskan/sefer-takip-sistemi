# İETT Sefer Planlama ve Raporlama Sistemi — Dokümantasyon

> Bu doküman, .NET Core (EF Core) ile geliştirilecek "İETT Sefer Planlama ve Mesai Yönetim Sistemi" için proje genel tasarımını, veri modellerini, DB yapılandırmasını, API uç noktalarını, raporlama gereksinimlerini ve geliştirme/dağıtım notlarını içerir.

---

## İçindekiler
1. Proje Özeti
2. Hedefler
3. Teknoloji Yığını
4. Mimari (Katmanlar)
5. Veri Modelleri (Türkçe)
6. DbContext & Zaman Damgaları
7. Örnek Migration / Seed Stratejisi
8. API Uç Noktaları (Özet)
9. Raporlama Özellikleri ve Filtreler
10. Örnek LINQ/SQL Rapor Sorguları
11. Servis Katmanı & Tasarım Notları
12. Loglama ve İzleme
13. Güvenlik Notları
14. Test & CI/CD
15. Dağıtım Önerileri
16. İleri Adımlar / Geliştirme Yol Haritası

---

## 1. Proje Özeti
Bu proje, İETT benzeri bir şehir içi toplu taşıma organizasyonu için "Sefer Planlama ve Mesai Yönetim Sistemi" oluşturmayı amaçlar. Proje; şoför, otobüs, hat yönetimi, sefer planlaması, bakım/arıza, yakıt ve yolcu sayımı kayıtlarını tutacak; kapsamlı raporlama (günlük, haftalık, aylık, yıllık, özel aralık, karşılaştırmalı) sunacaktır.

## 2. Hedefler
- Kolay yönetilebilir CRUD API'leri (şoför, otobüs, hat, sefer, bakım vb.)
- Tüm tablolarda `OlusturulmaTarihi` ve `GuncellenmeTarihi` alanları
- Esnek raporlama altyapısı; tarih aralığı, şoför/otobüs/hat filtreleme, karşılaştırmalar
- Sistem loglama (kullanıcı işlemleri)
- Seed veri ile hızlı geliştirme ortamı

## 3. Teknoloji Yığını
- Backend: .NET (ör. .NET 8 / .NET 7) + C#
- ORM: Entity Framework Core (Code-First)
- Veritabanı: PostgreSQL (veya MSSQL) — tercihe göre
- Frontend: React / Blazor / Vue (proje tercihi) — API ile iletişim
- Rapor Grafikleri: Chart.js / Recharts / Any JS Chart
- Ops: Docker, Docker Compose (geliştirme için)

## 4. Mimari (Katmanlar)
- **Domain**: Entity sınıfları (TemelEntity, Kullanici, Hat, Otobus, Sefer, BakimKaydi, YakitTuketim, YolcuSayim, SistemLog, SeferRapor vb.)
- **Infrastructure**: `IettDbContext`, Migration, Repository (opsiyonel)
- **Application**: Servisler, DTO'lar, Raporlama servisleri
- **API**: Controller'lar, Auth middleware, Exception handling

---

## 5. Veri Modelleri (Türkçe) — Özet
> Aşağıda önemli tablolar ve önemli alanlar yer almaktadır. (Kod örneği için proje içine alınabilir.)

### TemelEntity (Base)
```csharp
public abstract class TemelEntity
{
    public int Id { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    public DateTime? GuncellenmeTarihi { get; set; }
}
```

### Kullanici
```csharp
public class Kullanici : TemelEntity
{
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Sifre { get; set; } = null!; // Düz metin olarak saklanacak (önerilmez)
    public string Rol { get; set; } = "Kullanici"; // Sofor, Yonetici, Admin vb.

    public ICollection<Sefer> Seferler { get; set; } = new List<Sefer>();
    public ICollection<SistemLog> Loglar { get; set; } = new List<SistemLog>();
}
```

### Hat
```csharp
public class Hat : TemelEntity
{
    public string HatKodu { get; set; } = null!; // Ör: "34BZ"
    public string HatAdi { get; set; } = null!;
    public bool Durum { get; set; } = true; // true = aktif

    public ICollection<Sefer> Seferler { get; set; } = new List<Sefer>();
}
```

### Otobus
```csharp
public class Otobus : TemelEntity
{
    public string Plaka { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Kapasite { get; set; }

    public ICollection<Sefer> Seferler { get; set; } = new List<Sefer>();
}
```

### Sefer
```csharp
public class Sefer : TemelEntity
{
    public int HatId { get; set; }
    public Hat Hat { get; set; } = null!;

    public int SoforId { get; set; }
    public Kullanici Sofor { get; set; } = null!;

    public int OtobusId { get; set; }
    public Otobus Otobus { get; set; } = null!;

    public DateTime KalkisZamani { get; set; }
    public DateTime VarisZamani { get; set; }

    public string Durum { get; set; } = "Planlandi"; // Planlandi, Tamamlandi, Iptal
    public double GidilenMesafeKm { get; set; } = 0;
}
```

### BakimKaydi
```csharp
public class BakimKaydi : TemelEntity
{
    public int OtobusId { get; set; }
    public Otobus Otobus { get; set; } = null!;
    public DateTime BakimTarihi { get; set; }
    public string Aciklama { get; set; } = null!;
    public decimal Maliyet { get; set; }
}
```

### YakitTuketim
```csharp
public class YakitTuketim : TemelEntity
{
    public int OtobusId { get; set; }
    public Otobus Otobus { get; set; } = null!;
    public DateTime Tarih { get; set; }
    public double Litre { get; set; }
    public decimal Tutar { get; set; }
}
```

### YolcuSayim
```csharp
public class YolcuSayim : TemelEntity
{
    public int SeferId { get; set; }
    public Sefer Sefer { get; set; } = null!;
    public int YolcuSayisi { get; set; }
}
```

### SistemLog
```csharp
public class SistemLog : TemelEntity
{
    public int? KullaniciId { get; set; } // bazı loglar sistem kaynaklı olabilir
    public Kullanici? Kullanici { get; set; }
    public string Islem { get; set; } = null!;
    public string Detay { get; set; } = null!;
    public DateTime Tarih { get; set; } = DateTime.Now;
}
```

### SeferRapor (Opsiyonel, denormalize edilmiş rapor satırları)
```csharp
public class SeferRapor : TemelEntity
{
    public int HatId { get; set; }
    public int SoforId { get; set; }
    public int AracId { get; set; }

    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }

    public int YolcuSayisi { get; set; }
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public double ToplamMesafeKm { get; set; }
}
```

---

## 6. DbContext & Zaman Damgaları (SaveChanges Override)
`IettDbContext` içinde tüm `TemelEntity` türevleri için ekleme ve güncelleme sırasında tarihlerin otomatik set edilmesi önerilir.

```csharp
public override int SaveChanges()
{
    var entries = ChangeTracker.Entries<TemelEntity>();
    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
            entry.Entity.OlusturulmaTarihi = DateTime.UtcNow;

        if (entry.State == EntityState.Modified)
            entry.Entity.GuncellenmeTarihi = DateTime.UtcNow;
    }

    return base.SaveChanges();
}
```

Not: Asenkron çağrılar için `SaveChangesAsync` override edilmeli.

---

## 7. Örnek Migration / Seed Stratejisi
- `Add-Migration InitialCreate` + `Update-Database` ile tablolar oluşturulur.
- `SeedData` sınıfı ile başlangıç verileri: birkaç hat, birkaç otobüs, birkaç şoför, bir admin kullanıcı eklenir.
- Seed sırasında `Sifre` alanı düz metin olarak eklenir (test ortamı). Prod için hash önerilir.

---

## 8. API Uç Noktaları (Örnek)
Aşağıdaki uç noktalar CRUD operasyonları ve raporlama için önerilir.

### Kaynak CRUD
- `GET /api/hatlar` — filtreli liste (aktif/pasif, arama)
- `GET /api/hatlar/{id}`
- `POST /api/hatlar`
- `PUT /api/hatlar/{id}`
- `DELETE /api/hatlar/{id}`

Benzer şekilde `otobusler`, `kullanicilar`, `seferler`, `bakimlar`, `yakit` için CRUD.

### Raporlama
- `POST /api/rapor/seferler` — request body: `{ startDate, endDate, hatId?, soforId?, otobusId?, granularity }`
  - `granularity`: `daily|weekly|monthly|quarterly|yearly|custom`
- `GET /api/rapor/sofor/{soforId}?start=...&end=...&granularity=...`
- `GET /api/rapor/hat/{hatId}?start=...&end=...`
- `GET /api/rapor/karsilastir?hat1=...&hat2=...&start=...&end=...`

Response: JSON with gruplandırılmış veri ve özet metrikler (totalSefer, toplamYolcu, ortalamaHiz, toplamGelir vb.).

---

## 9. Raporlama Özellikleri (Detaylı)
Aşağıdaki rapor türleri implement edilebilir:

### Zaman Bazlı Raporlar
- Günlük (her bir gün için metrik)
- Haftalık (ISO haftası bazlı veya Pazartesi başlangıçlı)
- Aylık
- Çeyreklik
- Yıllık
- Özel aralık

### Filtrelere Göre
- Şoför bazlı
- Otobüs bazlı
- Hat bazlı
- Durum (Planlandı/Tamamlandı/İptal)
- Aktif/pasif hatlar

### Karşılaştırmalar
- Dönem karşılaştırmaları (önceki dönem vs mevcut)
- Hat vs hat
- Şoför vs şoför

### İleri KPI'lar
- Zamanında varış yüzdesi (on-time %)
- Ortalama doluluk (yolcu / kapasite)
- Yakıt verimliliği (km / litre veya litre / 100km)
- Sefer başına ortalama gelir
- Bakım maliyeti / km

---

## 10. Örnek LINQ / SQL Rapor Sorguları (Basit Örnekler)
**Toplam sefer ve yolcu sayısı (günlük)**
```csharp
var result = await _db.Seferler
    .Where(s => s.KalkisZamani >= start && s.KalkisZamani <= end)
    .GroupBy(s => s.KalkisZamani.Date)
    .Select(g => new {
        Tarih = g.Key,
        ToplamSefer = g.Count(),
        ToplamYolcu = g.Sum(s => s.YolcuSayimKaydi != null ? s.YolcuSayimKaydi.YolcuSayisi : 0)
    })
    .ToListAsync();
```

**Şoför bazlı aylık sefer sayısı**
```csharp
var result = await _db.Seferler
    .Where(s => s.SoforId == soforId && s.KalkisZamani >= start && s.KalkisZamani <= end)
    .GroupBy(s => new { s.KalkisZamani.Year, s.KalkisZamani.Month })
    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
    .ToListAsync();
```

**Hat karşılaştırması (aylık toplam yolcu)**
```csharp
var result = await _db.YolcuSayim
    .Where(y => y.Sefer.KalkisZamani >= start && y.Sefer.KalkisZamani <= end && (y.Sefer.HatId == hat1 || y.Sefer.HatId == hat2))
    .GroupBy(y => new { y.Sefer.HatId, y.Sefer.KalkisZamani.Month })
    .Select(g => new { HatId = g.Key.HatId, Month = g.Key.Month, Yolcu = g.Sum(x => x.YolcuSayisi) })
    .ToListAsync();
```

> Not: Ağır sorgular için `SeferRapor` gibi denormalize edilmiş tablolara periyodik (daily/hourly) ETL ile veri çekilip rapor sorguları hızlandırılabilir.

---

## 11. Servis Katmanı & Tasarım Notları
- **Repository pattern** opsiyonel (EF Core sorguları doğrudan servis katmanında okunabilir).
- **Rapor servisleri**: `IRaporService` arayüzü, farklı rapor tipleri için implementasyonlar (SeferRaporService, FinansRaporService vb.).
- **Background jobs**: Büyük veri setleri ve denormalizasyon için `Hangfire` veya `Quartz.NET` ile günlük rapor oluşturma işleri çalıştır.

---

## 12. Loglama ve İzleme
- API erişimleri, hata logları ve kullanıcı işlemleri `SistemLog` tablosuna yazılmalı.
- Uygulama logları için Serilog + Seq / ELK stack önerilir.

---

## 13. Güvenlik Notları
- **Önemli:** Sifreleri düz metin saklamak güvenlik riski yaratır. Üretim ortamında _kesinlikle_ hash (Argon2/BCrypt) kullanılmalı.
- API için JWT tabanlı authentication ve yetkilendirme (rol bazlı) önerilir.
- Sensitive alanlar ve connection string'ler `appsettings.json` yerine `User Secrets` (gel. ortamda env vars) ile yönetilmeli.

---

## 14. Test & CI/CD
- Unit testler: Servis katmanı için xUnit
- Integration test: In-memory DB veya TestContainer ile
- CI: GitHub Actions / GitLab CI ile build -> test -> docker image pipeline

---

## 15. Dağıtım Önerileri
- Dockerize edip, staging/prod için Docker Compose veya Kubernetes kullanımı
- Veritabanı: managed PostgreSQL veya Azure/AWS RDS
- Log/Monitoring: Prometheus + Grafana veya ELK / Seq

---

## 16. İleri Adımlar / Yol Haritası
1. Proje iskeleti oluştur: Domain, Infrastructure, Application, API
2. EF Core Code-First migration: InitialCreate
3. SeedData ile örnek veri yükle
4. Temel CRUD API'lerini yaz (hat, otobus, sofor, sefer)
5. Raporlama endpoint'lerini ekle ve örnek LINQ sorgularını implement et
6. Arayüz (frontend) ile rapor görselleştirme (Chart.js)
7. Background job ile denormalize edilmiş `SeferRapor` tablosunu periyodik oluştur
8. Güvenlik iyileştirmeleri (sifre hash, HTTPS, rate limiting)

---

### Ek: Notlar
- Kod örnekleri kolay okunur olması için basitleştirilmiştir; production-ready kodda validation, DTO'lar, Automapper, filtreleme/paging ve hata yönetimi eklenmelidir.
- İstersen bu doküman üzerinden otomatik olarak bir `README.md` + örnek `IettDbContext.cs` + `SeedData.cs` dosyalarını oluşturup proje klasörüne hazırlayabilirim.

---

Doküman hazır. İstersen bu dokümanı doğrudan bir `README.md` olarak indirilebilecek şekilde dosya haline getirip paylaşayım veya proje için kod iskeleti (migration + seed + örnek controller) oluşturmaya başlayayım.

