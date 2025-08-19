using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IettSeferSistemi.Infrastructure.Data;
using IettSeferSistemi.Domain.Entities;
using IettSeferSistemi.Domain.Enums;

namespace IettSeferSistemi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RaporlarController : ControllerBase
{
    private readonly IettDbContext _context;

    public RaporlarController(IettDbContext context)
    {
        _context = context;
    }

    // GET: api/Raporlar/test
    [HttpGet("test")]
    public async Task<ActionResult<object>> GetTest()
    {
        try
        {
            var seferCount = await _context.Seferler.CountAsync();
            var hatCount = await _context.Hatlar.CountAsync();
            
            return Ok(new { seferCount, hatCount, message = "Test başarılı" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/Raporlar/gunluk-ozet
    [HttpGet("gunluk-ozet")]
    public async Task<ActionResult<object>> GetGunlukOzet([FromQuery] DateTime? tarih, [FromQuery] int? hatId)
    {
        try
        {
            var hedefTarih = tarih ?? DateTime.Today;
            var gunBaslangic = hedefTarih.Date;
            var gunBitis = gunBaslangic.AddDays(1);

            // Simplified query without complex includes
            var query = _context.Seferler
                .Where(s => s.KalkisZamani >= gunBaslangic && s.KalkisZamani < gunBitis)
                .Take(100); // Limit results to prevent timeout

            if (hatId.HasValue)
            {
                query = query.Where(s => s.HatId == hatId.Value);
            }

            var seferler = await query
                .Select(s => new { s.HatId, s.Durum, s.KalkisZamani, s.VarisZamani })
                .ToListAsync();

            // Basic statistics only
            var toplamSefer = seferler.Count;
            var tamamlananSefer = seferler.Count(s => s.Durum == SeferDurum.Tamamlandi);
            var iptalEdilenSefer = seferler.Count(s => s.Durum == SeferDurum.IptalEdildi);
            var toplamYolcu = toplamSefer * 25; // Mock average passengers per trip
            var ortalamaSure = 45.0; // Static average duration

            // Simplified route-based grouping
            var hatBazliVeriler = seferler
                .GroupBy(s => s.HatId)
                .Select(g => new
                {
                    hatId = g.Key,
                    hatAdi = "Hat " + g.Key, // Simplified naming
                    seferSayisi = g.Count(),
                    tamamlananSefer = g.Count(s => s.Durum == SeferDurum.Tamamlandi),
                    toplamYolcu = g.Count() * 25, // Mock calculation
                    ortalamaSure = 45.0 // Static value
                })
                .OrderByDescending(x => x.seferSayisi)
                .Take(10) // Limit to top 10
                .ToList();

            // Simplified hourly distribution
            var saatlikDagilim = seferler
                .GroupBy(s => s.KalkisZamani.Hour)
                .Select(g => new
                {
                    saat = g.Key,
                    seferSayisi = g.Count(),
                    yolcuSayisi = g.Count() * 25 // Mock calculation
                })
                .OrderBy(x => x.saat)
                .ToList();

            return Ok(new
            {
                tarih = hedefTarih.ToString("dd.MM.yyyy"),
                genelIstatistikler = new
                {
                    toplamSefer,
                    tamamlananSefer,
                    iptalEdilenSefer,
                    toplamYolcu,
                    ortalamaSure
                },
                hatBazliVeriler,
                saatlikDagilim
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/Raporlar/sofor-aylik
    [HttpGet("sofor-aylik")]
    public async Task<ActionResult<object>> GetSoforAylikRapor([FromQuery] int? ay, [FromQuery] int? yil)
    {
        try
        {
            var hedefAy = ay ?? DateTime.Now.Month;
            var hedefYil = yil ?? DateTime.Now.Year;
            var ayBaslangic = new DateTime(hedefYil, hedefAy, 1);
            var ayBitis = ayBaslangic.AddMonths(1);

            // Simplified query without complex includes and calculations
            var soforRaporlari = await _context.Seferler
                .Where(s => s.KalkisZamani >= ayBaslangic && s.KalkisZamani < ayBitis)
                .GroupBy(s => s.SoforId)
                .Select(g => new
                {
                    soforId = g.Key,
                    toplamSefer = g.Count(),
                    tamamlananSefer = g.Count(s => s.Durum == SeferDurum.Tamamlandi),
                    iptalEdilenSefer = g.Count(s => s.Durum == SeferDurum.IptalEdildi),
                    calisilanGunSayisi = g.Select(s => s.KalkisZamani.Date).Distinct().Count()
                })
                .OrderByDescending(x => x.toplamSefer)
                .Take(20) // Limit to top 20 drivers
                .ToListAsync();

            return Ok(new
            {
                ay = hedefAy,
                yil = hedefYil,
                soforRaporlari = soforRaporlari.Select(s => new
                {
                    s.soforId,
                    soforAdi = $"Şoför {s.soforId}", // Simplified naming
                    s.toplamSefer,
                    s.tamamlananSefer,
                    s.iptalEdilenSefer,
                    toplamCalismaSaati = s.toplamSefer * 1.5, // Mock calculation
                    ortalamaSeferSuresi = 45.0, // Static average
                    s.calisilanGunSayisi,
                    basariOrani = s.toplamSefer > 0 ? Math.Round((double)s.tamamlananSefer / s.toplamSefer * 100, 1) : 0
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/Raporlar/hat-karsilastirma
    [HttpGet("hat-karsilastirma")]
    public async Task<ActionResult<object>> GetHatKarsilastirma([FromQuery] int hat1Id, [FromQuery] int hat2Id, [FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis)
    {
        try
        {
            var baslangicTarih = baslangic ?? DateTime.Today.AddDays(-30);
            var bitisTarih = bitis ?? DateTime.Today;

            // Simplified queries without complex includes
            var hat1Seferler = await _context.Seferler
                .Where(s => s.HatId == hat1Id && s.KalkisZamani >= baslangicTarih && s.KalkisZamani <= bitisTarih)
                .Select(s => new { s.Durum })
                .Take(100)
                .ToListAsync();

            var hat2Seferler = await _context.Seferler
                .Where(s => s.HatId == hat2Id && s.KalkisZamani >= baslangicTarih && s.KalkisZamani <= bitisTarih)
                .Select(s => new { s.Durum })
                .Take(100)
                .ToListAsync();

            var hat1Istatistik = new
            {
                hatId = hat1Id,
                hatAdi = $"Hat {hat1Id}", // Simplified naming
                toplamSefer = hat1Seferler.Count,
                tamamlananSefer = hat1Seferler.Count(s => s.Durum == SeferDurum.Tamamlandi),
                toplamYolcu = hat1Seferler.Count * 25, // Mock calculation
                ortalamaSure = 45.0, // Static average
                basariOrani = hat1Seferler.Count > 0 ?
                    (double)hat1Seferler.Count(s => s.Durum == SeferDurum.Tamamlandi) / hat1Seferler.Count * 100 : 0
            };

            var hat2Istatistik = new
            {
                hatId = hat2Id,
                hatAdi = $"Hat {hat2Id}", // Simplified naming
                toplamSefer = hat2Seferler.Count,
                tamamlananSefer = hat2Seferler.Count(s => s.Durum == SeferDurum.Tamamlandi),
                toplamYolcu = hat2Seferler.Count * 25, // Mock calculation
                ortalamaSure = 45.0, // Static average
                basariOrani = hat2Seferler.Count > 0 ?
                    (double)hat2Seferler.Count(s => s.Durum == SeferDurum.Tamamlandi) / hat2Seferler.Count * 100 : 0
            };

            return Ok(new
            {
                baslangicTarih = baslangicTarih.ToString("yyyy-MM-dd"),
                bitisTarih = bitisTarih.ToString("yyyy-MM-dd"),
                hat1 = new
                {
                    hat1Istatistik.hatId,
                    hat1Istatistik.hatAdi,
                    hat1Istatistik.toplamSefer,
                    hat1Istatistik.tamamlananSefer,
                    hat1Istatistik.toplamYolcu,
                    ortalamaSure = Math.Round(hat1Istatistik.ortalamaSure, 1),
                    basariOrani = Math.Round(hat1Istatistik.basariOrani, 1)
                },
                hat2 = new
                {
                    hat2Istatistik.hatId,
                    hat2Istatistik.hatAdi,
                    hat2Istatistik.toplamSefer,
                    hat2Istatistik.tamamlananSefer,
                    hat2Istatistik.toplamYolcu,
                    ortalamaSure = Math.Round(hat2Istatistik.ortalamaSure, 1),
                    basariOrani = Math.Round(hat2Istatistik.basariOrani, 1)
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/Raporlar/hat-performans
    [HttpGet("hat-performans")]
    public async Task<ActionResult<object>> GetHatPerformans([FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis)
    {
        try
        {
            var baslangicTarih = baslangic ?? DateTime.Today.AddDays(-30);
            var bitisTarih = bitis ?? DateTime.Today;

            // Simplified query without complex includes
            var result = await _context.Seferler
                .Where(s => s.KalkisZamani >= baslangicTarih && s.KalkisZamani <= bitisTarih)
                .GroupBy(s => s.HatId)
                .Select(g => new
                {
                    hatId = g.Key,
                    hat = "Hat " + g.Key, // Simplified naming
                    toplam = g.Count(),
                    tamamlanan = g.Count(s => s.Durum == SeferDurum.Tamamlandi),
                    iptal = g.Count(s => s.Durum == SeferDurum.IptalEdildi),
                    basariOrani = g.Count() > 0 ? Math.Round((double)g.Count(s => s.Durum == SeferDurum.Tamamlandi) / g.Count() * 100, 1) : 0
                })
                .OrderByDescending(x => x.toplam)
                .Take(20) // Limit to top 20 routes
                .ToListAsync();

            return Ok(new
            {
                baslangicTarih = baslangicTarih.ToString("yyyy-MM-dd"),
                bitisTarih = bitisTarih.ToString("yyyy-MM-dd"),
                hatPerformanslari = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/Raporlar/sofor-istatistik/{soforId}
    [HttpGet("sofor-istatistik/{soforId}")]
    public async Task<ActionResult<object>> GetSoforIstatistik(int soforId)
    {
        try
        {
            var sofor = await _context.Kullanicilar.FindAsync(soforId);
            if (sofor == null)
            {
                return NotFound(new { error = "Şoför bulunamadı" });
            }

            var bugun = DateTime.Today;
            var buAy = new DateTime(bugun.Year, bugun.Month, 1);
            var son7Gun = bugun.AddDays(-6);

            // Simplified query - only get basic stats to prevent timeout
            var buAyinSeferleri = await _context.Seferler
                .Where(s => s.SoforId == soforId && s.KalkisZamani >= buAy)
                .Select(s => new { s.KalkisZamani, s.VarisZamani })
                .ToListAsync();

            // Basic statistics - calculate real working hours and days
            var toplamSeferSayisi = buAyinSeferleri.Count;
            double toplamCalismaSaati = 0.0;
            var calisilanGunler = new HashSet<DateTime>();
            
            if (toplamSeferSayisi > 0)
            {
                // Calculate total working hours from actual trip data
                foreach (var sefer in buAyinSeferleri)
                {
                    // Add working day to set
                    calisilanGunler.Add(sefer.KalkisZamani.Date);
                    
                    if (sefer.VarisZamani.HasValue)
                    {
                        toplamCalismaSaati += (sefer.VarisZamani.Value - sefer.KalkisZamani).TotalHours;
                    }
                    else
                    {
                        toplamCalismaSaati += 1.5; // Default for trips without end time
                    }
                }
            }
            
            var toplamCalisilanGun = calisilanGunler.Count;
            
            var istatistikler = new
            {
                toplamCalismaSaati = Math.Round(toplamCalismaSaati, 1),
                toplamSeferSayisi = toplamSeferSayisi,
                toplamCalisilanGun = toplamCalisilanGun,
                toplamCalismaSaati_label = $"{Math.Round(toplamCalismaSaati, 1)} saat",
                ortalamaSeferSuresi = toplamSeferSayisi > 0 ? Math.Round(toplamCalismaSaati * 60 / toplamSeferSayisi, 0) : 0 // Average minutes per trip
            };

            // Simplified monthly data (last 6 months only)
            var son6Ay = bugun.AddMonths(-5);
            var son6AyBaslangic = new DateTime(son6Ay.Year, son6Ay.Month, 1);
            
            var aylikSeferData = await _context.Seferler
                .Where(s => s.SoforId == soforId && s.KalkisZamani >= son6AyBaslangic)
                .GroupBy(s => new { s.KalkisZamani.Year, s.KalkisZamani.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();
            
            var aylikSeferler = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var ay = bugun.AddMonths(-i);
                var seferSayisi = aylikSeferData
                    .Where(x => x.Year == ay.Year && x.Month == ay.Month)
                    .FirstOrDefault()?.Count ?? 0;
                
                aylikSeferler.Add(new
                {
                    ay = ay.Month,
                    yil = ay.Year,
                    ayAdi = GetMonthName(ay.Month),
                    seferSayisi = seferSayisi
                });
            }

            // Calculate real weekly working hours for current month
            var haftalikSaatler = new List<object>();
            var ayBaslangici = new DateTime(bugun.Year, bugun.Month, 1);
            var aySonu = ayBaslangici.AddMonths(1).AddDays(-1);
            
            // Divide month into 4 weeks
            for (int hafta = 1; hafta <= 4; hafta++)
            {
                var haftaBaslangici = ayBaslangici.AddDays((hafta - 1) * 7);
                var haftaSonu = haftaBaslangici.AddDays(6);
                
                // Ensure we don't go beyond month end
                if (haftaSonu > aySonu)
                    haftaSonu = aySonu;
                
                // Calculate working hours for this week
                var haftaninSeferleri = buAyinSeferleri.Where(s => 
                    s.KalkisZamani.Date >= haftaBaslangici.Date && 
                    s.KalkisZamani.Date <= haftaSonu.Date).ToList();
                
                double haftaninSaatleri = 0;
                foreach (var sefer in haftaninSeferleri)
                {
                    if (sefer.VarisZamani.HasValue)
                    {
                        var sure = (sefer.VarisZamani.Value - sefer.KalkisZamani).TotalHours;
                        haftaninSaatleri += sure;
                    }
                }
                
                haftalikSaatler.Add(new
                {
                    hafta = hafta,
                    saat = Math.Round(haftaninSaatleri, 1)
                });
            }

            // Simplified daily stats (last 7 days) - only show if driver has trips
            var detayliIstatistikler = new List<object>();
            
            // Only show detailed statistics if driver has any trips this month
            if (buAyinSeferleri.Count > 0)
            {
                // Get detailed trip data for accurate working hours calculation
                var gunlukSeferDetay = await _context.Seferler
                    .Include(s => s.Hat)
                    .Where(s => s.SoforId == soforId && s.KalkisZamani.Date >= son7Gun.Date)
                    .Select(s => new {
                        s.KalkisZamani,
                        s.VarisZamani,
                        HatKodu = s.Hat != null ? s.Hat.HatKodu : "Bilinmeyen Hat"
                    })
                    .ToListAsync();
                

                
                // Group by date and calculate real working hours
                var gunlukSeferDataWithHours = gunlukSeferDetay
                    .GroupBy(s => s.KalkisZamani.Date)
                    .Select(g => {
                        var seferler = g.OrderBy(x => x.KalkisZamani).ToList();
                        double toplamSure = 0;
                        
                        if (seferler.Count == 1)
                        {
                            // Tek sefer varsa, o seferin kendi süresini kullan
                            var sefer = seferler.First();
                            if (sefer.VarisZamani.HasValue)
                            {
                                toplamSure = (sefer.VarisZamani.Value - sefer.KalkisZamani).TotalHours;
                            }
                            else
                            {
                                toplamSure = 1.5; // Varsayılan sefer süresi
                            }
                        }
                        else
                        {
                            // Birden fazla sefer varsa, ilk seferin başlangıcı ile son seferin bitişi arasındaki süre
                            var ilkSefer = seferler.First().KalkisZamani;
                            var sonSefer = seferler.Last().VarisZamani ?? seferler.Last().KalkisZamani.AddHours(1.5);
                            toplamSure = (sonSefer - ilkSefer).TotalHours;
                        }
                        
                        return new {
                            Tarih = g.Key,
                            SeferSayisi = g.Count(),
                            HatAdi = g.First().HatKodu,
                            ToplamSure = Math.Max(toplamSure, g.Count() * 0.5) // Minimum 30 min per trip
                        };
                    })
                    .OrderBy(x => x.Tarih)
                    .ToList();
                
                // Only show days where driver actually worked (has trips)
                foreach (var gunVerisi in gunlukSeferDataWithHours.Where(x => x.SeferSayisi > 0))
                {
                    // Determine status based on working hours and trip count
                    string durum;
                    if (gunVerisi.ToplamSure >= 8 && gunVerisi.SeferSayisi >= 5)
                    {
                        durum = "tamamlandi";
                    }
                    else if (gunVerisi.ToplamSure >= 4 && gunVerisi.SeferSayisi >= 3)
                    {
                        durum = "aktif";
                    }
                    else if (gunVerisi.SeferSayisi >= 1)
                    {
                        durum = "planli";
                    }
                    else
                    {
                        durum = "iptal";
                    }
                    
                    detayliIstatistikler.Add(new
                    {
                        tarih = gunVerisi.Tarih.ToString("dd.MM.yyyy"),
                        gun = GetDayName(gunVerisi.Tarih.DayOfWeek),
                        vardiya = gunVerisi.HatAdi,
                        seferSayisi = gunVerisi.SeferSayisi,
                        calismaSaati = Math.Round(gunVerisi.ToplamSure, 1) + " saat",
                        durum = durum
                    });
                }
            }

            return Ok(new
            {
                istatistikler,
                aylikSeferler,
                haftalikSaatler,
                detayliIstatistikler
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static string GetMonthName(int month)
    {
        return month switch
        {
            1 => "Ocak",
            2 => "Şubat",
            3 => "Mart",
            4 => "Nisan",
            5 => "Mayıs",
            6 => "Haziran",
            7 => "Temmuz",
            8 => "Ağustos",
            9 => "Eylül",
            10 => "Ekim",
            11 => "Kasım",
            12 => "Aralık",
            _ => "Bilinmeyen"
        };
    }

    private static string GetDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Pazartesi",
            DayOfWeek.Tuesday => "Salı",
            DayOfWeek.Wednesday => "Çarşamba",
            DayOfWeek.Thursday => "Perşembe",
            DayOfWeek.Friday => "Cuma",
            DayOfWeek.Saturday => "Cumartesi",
            DayOfWeek.Sunday => "Pazar",
            _ => "Bilinmeyen"
        };
    }

    [HttpGet("sofor-istatistik/{soforId}/{yil}/{ay}")]
    public async Task<IActionResult> GetSoforIstatistikByMonth(int soforId, int yil, int ay)
    {
        try
        {
            var seciliAy = new DateTime(yil, ay, 1);
            var seciliAyinSonu = seciliAy.AddMonths(1).AddDays(-1);
            
            // Get trips for selected month
            var seciliAyinSeferleri = await _context.Seferler
                .Where(s => s.SoforId == soforId && s.KalkisZamani >= seciliAy && s.KalkisZamani <= seciliAyinSonu)
                .Select(s => new { s.KalkisZamani, s.VarisZamani })
                .ToListAsync();

            // Calculate weekly hours for selected month
            var haftalikSaatler = new List<object>();
            for (int hafta = 1; hafta <= 4; hafta++)
            {
                var haftaBaslangic = seciliAy.AddDays((hafta - 1) * 7);
                var haftaBitis = haftaBaslangic.AddDays(6);
                
                var haftaninSeferleri = seciliAyinSeferleri
                    .Where(s => s.KalkisZamani.Date >= haftaBaslangic.Date && s.KalkisZamani.Date <= haftaBitis.Date)
                    .ToList();
                
                double haftaninSaatleri = 0.0;
                foreach (var sefer in haftaninSeferleri)
                {
                    if (sefer.VarisZamani.HasValue)
                    {
                        haftaninSaatleri += (sefer.VarisZamani.Value - sefer.KalkisZamani).TotalHours;
                    }
                    else
                    {
                        haftaninSaatleri += 1.5;
                    }
                }
                
                haftalikSaatler.Add(new
                {
                    hafta = hafta,
                    saat = Math.Round(haftaninSaatleri, 1)
                });
            }

            return Ok(new
            {
                yil = yil,
                ay = ay,
                ayAdi = GetMonthName(ay),
                haftalikSaatler = haftalikSaatler
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}