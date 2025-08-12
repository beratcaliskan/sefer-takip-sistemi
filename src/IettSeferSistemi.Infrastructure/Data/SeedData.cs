using IettSeferSistemi.Domain.Entities;
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
                Email = "ahmet.yilmaz@iett.gov.tr",
                Sifre = "123456", // Üretimde hash'lenmiş olmalı
                Rol = "Sofor"
            },
            new Kullanici
            {
                Ad = "Mehmet",
                Soyad = "Demir",
                Email = "mehmet.demir@iett.gov.tr",
                Sifre = "123456",
                Rol = "Sofor"
            },
            new Kullanici
            {
                Ad = "Fatma",
                Soyad = "Kaya",
                Email = "fatma.kaya@iett.gov.tr",
                Sifre = "123456",
                Rol = "Yonetici"
            },
            new Kullanici
            {
                Ad = "Admin",
                Soyad = "User",
                Email = "admin@iett.gov.tr",
                Sifre = "admin123",
                Rol = "Admin"
            }
        };

        context.Kullanicilar.AddRange(kullanicilar);
        await context.SaveChangesAsync();

        // Hatlar
        var hatlar = new List<Hat>
        {
            new Hat { HatKodu = "34BZ", HatAdi = "Beşiktaş - Zincirlikuyu", Durum = true },
            new Hat { HatKodu = "15F", HatAdi = "Eminönü - Fatih", Durum = true },
            new Hat { HatKodu = "28T", HatAdi = "Taksim - Topkapı", Durum = true },
            new Hat { HatKodu = "42K", HatAdi = "Kadıköy - Kartal", Durum = true },
            new Hat { HatKodu = "55Ü", HatAdi = "Üsküdar - Ümraniye", Durum = false }
        };

        context.Hatlar.AddRange(hatlar);
        await context.SaveChangesAsync();

        // Otobüsler
        var otobusler = new List<Otobus>
        {
            new Otobus { Plaka = "34 ABC 123", Model = "Mercedes Citaro", Kapasite = 90 },
            new Otobus { Plaka = "34 DEF 456", Model = "MAN Lion's City", Kapasite = 85 },
            new Otobus { Plaka = "34 GHI 789", Model = "Isuzu Citiport", Kapasite = 75 },
            new Otobus { Plaka = "34 JKL 012", Model = "BMC Procity", Kapasite = 80 },
            new Otobus { Plaka = "34 MNO 345", Model = "Temsa Avenue", Kapasite = 95 }
        };

        context.Otobusler.AddRange(otobusler);
        await context.SaveChangesAsync();

        // Örnek Seferler
        var seferler = new List<Sefer>
        {
            new Sefer
            {
                HatId = hatlar[0].Id,
                SoforId = kullanicilar[0].Id,
                OtobusId = otobusler[0].Id,
                KalkisZamani = DateTime.Now.AddHours(-2),
                VarisZamani = DateTime.Now.AddHours(-1),
                Durum = "Tamamlandi",
                GidilenMesafeKm = 15.5
            },
            new Sefer
            {
                HatId = hatlar[1].Id,
                SoforId = kullanicilar[1].Id,
                OtobusId = otobusler[1].Id,
                KalkisZamani = DateTime.Now.AddMinutes(-30),
                VarisZamani = DateTime.Now.AddMinutes(30),
                Durum = "Planlandi",
                GidilenMesafeKm = 12.3
            }
        };

        context.Seferler.AddRange(seferler);
        await context.SaveChangesAsync();

        // Örnek Yolcu Sayımları
        var yolcuSayimlari = new List<YolcuSayim>
        {
            new YolcuSayim { SeferId = seferler[0].Id, YolcuSayisi = 45 },
            new YolcuSayim { SeferId = seferler[1].Id, YolcuSayisi = 32 }
        };

        context.YolcuSayimlari.AddRange(yolcuSayimlari);
        await context.SaveChangesAsync();

        // Örnek Bakım Kayıtları
        var bakimKayitlari = new List<BakimKaydi>
        {
            new BakimKaydi
            {
                OtobusId = otobusler[0].Id,
                BakimTarihi = DateTime.Now.AddDays(-7),
                Aciklama = "Periyodik bakım - Yağ değişimi",
                Maliyet = 850.00m
            },
            new BakimKaydi
            {
                OtobusId = otobusler[1].Id,
                BakimTarihi = DateTime.Now.AddDays(-3),
                Aciklama = "Fren balata değişimi",
                Maliyet = 1200.00m
            }
        };

        context.BakimKayitlari.AddRange(bakimKayitlari);
        await context.SaveChangesAsync();

        // Örnek Yakıt Tüketimleri
        var yakitTuketimleri = new List<YakitTuketim>
        {
            new YakitTuketim
            {
                OtobusId = otobusler[0].Id,
                Tarih = DateTime.Now.AddDays(-1),
                Litre = 120.5,
                Tutar = 3615.00m
            },
            new YakitTuketim
            {
                OtobusId = otobusler[1].Id,
                Tarih = DateTime.Now.AddDays(-2),
                Litre = 95.3,
                Tutar = 2859.00m
            }
        };

        context.YakitTuketimleri.AddRange(yakitTuketimleri);
        await context.SaveChangesAsync();

        // Sistem Logları
        var sistemLoglari = new List<SistemLog>
        {
            new SistemLog
            {
                KullaniciId = kullanicilar[2].Id,
                Islem = "Sefer Oluşturma",
                Detay = "Yeni sefer planlandı: 34BZ hattı",
                Tarih = DateTime.Now.AddHours(-1)
            },
            new SistemLog
            {
                KullaniciId = kullanicilar[3].Id,
                Islem = "Sistem Girişi",
                Detay = "Admin kullanıcısı sisteme giriş yaptı",
                Tarih = DateTime.Now.AddMinutes(-30)
            }
        };

        context.SistemLoglari.AddRange(sistemLoglari);
        await context.SaveChangesAsync();
    }
}