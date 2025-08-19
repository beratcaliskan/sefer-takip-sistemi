using Microsoft.EntityFrameworkCore;
using IettSeferSistemi.Domain.Entities;
using IettSeferSistemi.Domain.Enums;
using IettSeferSistemi.Infrastructure.Data;

namespace IettSeferSistemi.API.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new IettDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<IettDbContext>>()))
            {
                // Veritabanının oluşturulduğundan emin ol
                context.Database.EnsureCreated();

                // Eğer zaten veri varsa, seed işlemini yapma
                if (context.Hatlar.Any())
                {
                    return;
                }

                // Test kullanıcıları (şoförler) ekle
                var kullanicilar = new Kullanici[]
                {
                    new Kullanici
                    {
                        Ad = "Ahmet",
                        Soyad = "Yılmaz",
                        KullaniciAdi = "ahmet.yilmaz",
                        Email = "ahmet.yilmaz@iett.gov.tr",
                        Sifre = "123456",
                        Rol = "Sofor",
                        OlusturulmaTarihi = DateTime.Now
                    },
                    new Kullanici
                    {
                        Ad = "Mehmet",
                        Soyad = "Demir",
                        KullaniciAdi = "mehmet.demir",
                        Email = "mehmet.demir@iett.gov.tr",
                        Sifre = "123456",
                        Rol = "Sofor",
                        OlusturulmaTarihi = DateTime.Now
                    }
                };

                context.Kullanicilar.AddRange(kullanicilar);
                context.SaveChanges();

                // Test hatları ekle
                var hatlar = new Hat[]
                {
                    new Hat
                    {
                        HatKodu = "34BZ",
                        BaslangicDuragi = "Beşiktaş",
                        BitisDuragi = "Zincirlikuyu",
                        MesafeKm = 12.5,
                        TahminSureDakika = 45,
                        DurakSayisi = 18,
                        Aciklama = "Boğaziçi güzergahı",
                        Durum = true,
                        OlusturulmaTarihi = DateTime.Now
                    },
                    new Hat
                    {
                        HatKodu = "15F",
                        BaslangicDuragi = "Eminönü",
                        BitisDuragi = "Fatih",
                        MesafeKm = 8.2,
                        TahminSureDakika = 30,
                        DurakSayisi = 12,
                        Aciklama = "Tarihi yarımada hattı",
                        Durum = true,
                        OlusturulmaTarihi = DateTime.Now
                    }
                };

                context.Hatlar.AddRange(hatlar);
                context.SaveChanges();

                // Test otobüsleri ekle
                var otobusler = new Otobus[]
                {
                    new Otobus
                    {
                        Plaka = "34 ABC 123",
                        Model = "Mercedes Citaro",
                        Kapasite = 80,
                        OlusturulmaTarihi = DateTime.Now
                    },
                    new Otobus
                    {
                        Plaka = "34 DEF 456",
                        Model = "MAN Lion's City",
                        Kapasite = 90,
                        OlusturulmaTarihi = DateTime.Now
                    }
                };

                context.Otobusler.AddRange(otobusler);
                context.SaveChanges();

                // Test seferleri ekle
                var seferler = new Sefer[]
                {
                    new Sefer
                    {
                        HatId = hatlar[0].Id,
                        SoforId = kullanicilar[0].Id,
                        OtobusId = otobusler[0].Id,
                        KalkisZamani = DateTime.Today.AddHours(7),
                        VarisZamani = DateTime.Today.AddHours(18),
                        Durum = SeferDurum.Tamamlandi,
                        OlusturulmaTarihi = DateTime.Now,
                        GidilenMesafeKm = 150.5
                    },
                    new Sefer
                    {
                        HatId = hatlar[1].Id,
                        SoforId = kullanicilar[1].Id,
                        OtobusId = otobusler[1].Id,
                        KalkisZamani = DateTime.Today.AddHours(9),
                        VarisZamani = DateTime.Today.AddHours(17),
                        Durum = SeferDurum.Tamamlandi,
                        OlusturulmaTarihi = DateTime.Now,
                        GidilenMesafeKm = 120.3
                    }
                };

                context.Seferler.AddRange(seferler);
                context.SaveChanges();
            }
        }
    }
}