using IettSeferSistemi.Domain.Entities;
using IettSeferSistemi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IettSeferSistemi.Infrastructure.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = new IettDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<IettDbContext>>());

        // Veritabanının oluşturulduğundan emin ol
        await context.Database.EnsureCreatedAsync();

        // Eğer veri varsa, seed işlemini atla
        if (context.Kullanicilar.Any())
        {
            return; // DB zaten seed edilmiş
        }

        // Kullanıcılar (Şoförler ve Yöneticiler)
        var kullanicilar = new List<Kullanici>
        {
            new Kullanici
            {
                Ad = "Ahmet",
                Soyad = "Yılmaz",
                KullaniciAdi = "ahmet.yilmaz",
                Email = "ahmet.yilmaz@iett.gov.tr",
                Sifre = "123456",
                Rol = "Sofor"
            },
            new Kullanici
            {
                Ad = "Mehmet",
                Soyad = "Demir",
                KullaniciAdi = "mehmet.demir",
                Email = "mehmet.demir@iett.gov.tr",
                Sifre = "123456",
                Rol = "Sofor"
            },
            new Kullanici
            {
                Ad = "Ali",
                Soyad = "Kaya",
                KullaniciAdi = "ali.kaya",
                Email = "ali.kaya@iett.gov.tr",
                Sifre = "123456",
                Rol = "Sofor"
            },
            new Kullanici
            {
                Ad = "Fatma",
                Soyad = "Özkan",
                KullaniciAdi = "fatma.ozkan",
                Email = "fatma.ozkan@iett.gov.tr",
                Sifre = "admin123",
                Rol = "Yonetici"
            },
            new Kullanici
            {
                Ad = "Zeynep",
                Soyad = "Çelik",
                KullaniciAdi = "zeynep.celik",
                Email = "zeynep.celik@iett.gov.tr",
                Sifre = "admin123",
                Rol = "Admin"
            }
        };

        context.Kullanicilar.AddRange(kullanicilar);
        await context.SaveChangesAsync();

        // Hatlar
        var hatlar = new List<Hat>
        {
            new Hat { HatKodu = "15", BaslangicDuragi = "Eminönü", BitisDuragi = "Bağcılar", Durum = true },
            new Hat { HatKodu = "34", BaslangicDuragi = "Zincirlikuyu", BitisDuragi = "Mecidiyeköy", Durum = true },
            new Hat { HatKodu = "500T", BaslangicDuragi = "Taksim", BitisDuragi = "Beylikdüzü", Durum = true },
            new Hat { HatKodu = "BN1", BaslangicDuragi = "Beşiktaş", BitisDuragi = "Etiler", Durum = false }
        };

        context.Hatlar.AddRange(hatlar);
        await context.SaveChangesAsync();

        // Otobüsler
        var otobusler = new List<Otobus>
        {
            new Otobus { Plaka = "34 AB 123", Model = "Mercedes Citaro", Kapasite = 95 },
            new Otobus { Plaka = "34 CD 456", Model = "MAN Lion's City", Kapasite = 75 },
            new Otobus { Plaka = "34 EF 789", Model = "Isuzu Citiport", Kapasite = 120 },
            new Otobus { Plaka = "34 GH 012", Model = "BMC Procity", Kapasite = 45 }
        };

        context.Otobusler.AddRange(otobusler);
        await context.SaveChangesAsync();

        // Seferler - Son 3 ay için kapsamlı veri
        var seferler = new List<Sefer>();
        var random = new Random();
        var soforler = kullanicilar.Where(k => k.Rol == "Sofor").ToList();
        
        // Şoför ID 1 için özel sefer - bugünün tarihi
        seferler.Add(new Sefer
        {
            HatId = hatlar[0].Id,
            SoforId = soforler[0].Id,
            OtobusId = otobusler[0].Id,
            KalkisZamani = DateTime.Today.AddHours(7),
            VarisZamani = DateTime.Today.AddHours(18),
            Durum = SeferDurum.Tamamlandi,
            GidilenMesafeKm = 45.5
        });
        
        // Son 90 gün için sefer verileri oluştur
        for (int gun = -90; gun <= 0; gun++)
        {
            var tarih = DateTime.Today.AddDays(gun);
            
            // Hafta sonu kontrolü - hafta sonu daha az sefer
            var seferSayisi = tarih.DayOfWeek == DayOfWeek.Saturday || tarih.DayOfWeek == DayOfWeek.Sunday ? 
                random.Next(2, 5) : random.Next(4, 8);
            
            for (int i = 0; i < seferSayisi; i++)
            {
                var sofor = soforler[random.Next(soforler.Count)];
                var hat = hatlar[random.Next(hatlar.Count)];
                var otobus = otobusler[random.Next(otobusler.Count)];
                
                // Çalışma saatleri: 06:00 - 22:00 arası
                var kalkisSaat = random.Next(6, 20);
                var kalkisDakika = random.Next(0, 60);
                var seferSuresi = random.Next(45, 180); // 45-180 dakika arası
                
                var kalkisZamani = tarih.AddHours(kalkisSaat).AddMinutes(kalkisDakika);
                var varisZamani = kalkisZamani.AddMinutes(seferSuresi);
                
                // Durum belirleme - %85 tamamlandı, %10 devam ediyor, %5 iptal
                var durumRandom = random.Next(100);
                var durum = durumRandom < 85 ? SeferDurum.Tamamlandi : 
                           durumRandom < 95 ? SeferDurum.Devam : SeferDurum.IptalEdildi;
                
                // Mesafe hesaplama
                var mesafe = durum == SeferDurum.IptalEdildi ? 
                    random.Next(5, 15) : random.Next(15, 50);
                
                seferler.Add(new Sefer
                {
                    HatId = hat.Id,
                    SoforId = sofor.Id,
                    OtobusId = otobus.Id,
                    KalkisZamani = kalkisZamani,
                    VarisZamani = varisZamani,
                    Durum = durum,
                    GidilenMesafeKm = mesafe + (random.NextDouble() * 10) // Ondalık kısım için
                });
            }
        }

        context.Seferler.AddRange(seferler);
        await context.SaveChangesAsync();

        // Yolcu Sayımları - Her sefer için rastgele yolcu sayımları
        var yolcuSayimlari = new List<YolcuSayim>();
        
        foreach (var sefer in seferler.Where(s => s.Durum == SeferDurum.Tamamlandi))
        {
            // Her tamamlanan sefer için 1-4 arası yolcu sayımı
            var sayimSayisi = random.Next(1, 5);
            
            for (int i = 0; i < sayimSayisi; i++)
            {
                // Yolcu sayısı sefer durumuna ve saatine göre değişir
                var saat = sefer.KalkisZamani.Hour;
                var yolcuSayisi = 0;
                
                // Rush hour (07-09, 17-19) daha kalabalık
                if ((saat >= 7 && saat <= 9) || (saat >= 17 && saat <= 19))
                {
                    yolcuSayisi = random.Next(40, 85);
                }
                // Gündüz saatleri orta kalabalık
                else if (saat >= 10 && saat <= 16)
                {
                    yolcuSayisi = random.Next(20, 50);
                }
                // Akşam ve gece daha az
                else
                {
                    yolcuSayisi = random.Next(5, 30);
                }
                
                yolcuSayimlari.Add(new YolcuSayim
                {
                    SeferId = sefer.Id,
                    YolcuSayisi = yolcuSayisi
                });
            }
        }

        context.YolcuSayimlari.AddRange(yolcuSayimlari);
        await context.SaveChangesAsync();

        // Bakım Kayıtları
        var bakimKayitlari = new List<BakimKaydi>
        {
            new BakimKaydi
            {
                OtobusId = otobusler[0].Id,
                BakimTarihi = DateTime.Today.AddDays(-15),
                Aciklama = "Periyodik bakım ve yağ değişimi",
                Maliyet = 850.50m
            },
            new BakimKaydi
            {
                OtobusId = otobusler[1].Id,
                BakimTarihi = DateTime.Today.AddDays(-8),
                Aciklama = "Fren balata değişimi",
                Maliyet = 1200.75m
            },
            new BakimKaydi
            {
                OtobusId = otobusler[2].Id,
                BakimTarihi = DateTime.Today.AddDays(-3),
                Aciklama = "Klima sistemi tamiri",
                Maliyet = 650.00m
            }
        };

        context.BakimKayitlari.AddRange(bakimKayitlari);
        await context.SaveChangesAsync();

        // Yakıt Tüketimleri
        var yakitTuketimleri = new List<YakitTuketim>
        {
            new YakitTuketim
            {
                OtobusId = otobusler[0].Id,
                Tarih = DateTime.Today.AddDays(-2),
                Litre = 85.5,
                Tutar = 2565.00m
            },
            new YakitTuketim
            {
                OtobusId = otobusler[1].Id,
                Tarih = DateTime.Today.AddDays(-1),
                Litre = 72.3,
                Tutar = 2169.00m
            },
            new YakitTuketim
            {
                OtobusId = otobusler[2].Id,
                Tarih = DateTime.Today,
                Litre = 95.8,
                Tutar = 2874.00m
            },
            new YakitTuketim
            {
                OtobusId = otobusler[3].Id,
                Tarih = DateTime.Today.AddDays(-3),
                Litre = 45.2,
                Tutar = 1356.00m
            }
        };

        context.YakitTuketimleri.AddRange(yakitTuketimleri);
        await context.SaveChangesAsync();

        // Sistem Logları
        var sistemLoglari = new List<SistemLog>
        {
            new SistemLog
            {
                KullaniciId = kullanicilar[0].Id,
                Islem = "Giriş",
                Detay = "Kullanıcı sisteme giriş yaptı",
                Tarih = DateTime.Now.AddHours(-2)
            },
            new SistemLog
            {
                KullaniciId = kullanicilar[1].Id,
                Islem = "Sefer Tamamlama",
                Detay = "34 numaralı hat seferi tamamlandı",
                Tarih = DateTime.Now.AddHours(-1)
            },
            new SistemLog
            {
                KullaniciId = kullanicilar[3].Id,
                Islem = "Rapor Görüntüleme",
                Detay = "Aylık performans raporu görüntülendi",
                Tarih = DateTime.Now.AddMinutes(-30)
            }
        };

        context.SistemLoglari.AddRange(sistemLoglari);
        await context.SaveChangesAsync();
    }
}