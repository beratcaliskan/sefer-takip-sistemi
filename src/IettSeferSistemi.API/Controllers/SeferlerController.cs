using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IettSeferSistemi.Infrastructure.Data;
using IettSeferSistemi.Domain.Entities;
using IettSeferSistemi.Domain.Enums;

namespace IettSeferSistemi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeferlerController : ControllerBase
{
    private readonly IettDbContext _context;

    public SeferlerController(IettDbContext context)
    {
        _context = context;
    }

    // GET: api/Seferler/check-ids/{ids}
    [HttpGet("check-ids/{ids}")]
    public async Task<ActionResult<object>> CheckSeferIds(string ids)
    {
        try
        {
            var idList = ids.Split(',').Select(int.Parse).ToList();
            
            var seferler = await _context.Seferler
                .Where(s => idList.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.KalkisZamani,
                    s.VarisZamani,
                    Durum = (int)s.Durum,
                    s.GidilenMesafeKm
                })
                .ToListAsync();

            var totalCount = await _context.Seferler.CountAsync();
            var maxId = await _context.Seferler.MaxAsync(s => (int?)s.Id) ?? 0;
            var bigIds = await _context.Seferler
                .Where(s => s.Id > 450)
                .Select(s => s.Id)
                .OrderBy(s => s)
                .ToListAsync();

            return Ok(new
            {
                RequestedIds = idList,
                FoundSeferler = seferler,
                TotalSeferCount = totalCount,
                MaxSeferId = maxId,
                IdsGreaterThan450 = bigIds
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET: api/Seferler
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetSeferler()
    {
        try
        {
            // Highly optimized query - removed expensive subqueries
            var seferler = await _context.Seferler
                .Include(s => s.Hat)
                .Include(s => s.Sofor)
                .Include(s => s.Otobus)
                .OrderByDescending(s => s.KalkisZamani)
                .Take(50) // Further reduced limit for better performance
                .Select(s => new
                {
                    s.Id,
                    s.KalkisZamani,
                    s.VarisZamani,
                    Durum = (int)s.Durum,
                    s.GidilenMesafeKm,
                    Hat = s.Hat != null ? new { s.Hat.Id, s.Hat.HatKodu, HatAdi = s.Hat.BaslangicDuragi + " - " + s.Hat.BitisDuragi } : null,
                    Sofor = s.Sofor != null ? new { s.Sofor.Id, Ad = s.Sofor.Ad + " " + s.Sofor.Soyad } : null,
                    Otobus = s.Otobus != null ? new { s.Otobus.Id, s.Otobus.Plaka, s.Otobus.Model } : null
                    // Removed YolcuSayisi, Durak, YolcuNotlari to prevent timeout
                })
                .ToListAsync();

            return Ok(seferler);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/Seferler/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSefer(int id)
    {
        var sefer = await _context.Seferler
            .Include(s => s.Hat)
            .Include(s => s.Sofor)
            .Include(s => s.Otobus)
            .Include(s => s.YolcuSayimlari)
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.HatId,
                s.SoforId,
                s.OtobusId,
                s.KalkisZamani,
                s.VarisZamani,
                Durum = (int)s.Durum,
                s.GidilenMesafeKm,
                Hat = s.Hat != null ? new { s.Hat.Id, s.Hat.HatKodu, HatAdi = s.Hat.BaslangicDuragi + " - " + s.Hat.BitisDuragi } : null,
                Sofor = s.Sofor != null ? new { s.Sofor.Id, Ad = s.Sofor.Ad + " " + s.Sofor.Soyad } : null,
                Otobus = s.Otobus != null ? new { s.Otobus.Id, s.Otobus.Plaka, s.Otobus.Model } : null,
                YolcuSayisi = s.YolcuSayimlari.Sum(y => y.YolcuSayisi),
                Durak = s.YolcuSayimlari.OrderByDescending(y => y.OlusturulmaTarihi).FirstOrDefault() != null ? s.YolcuSayimlari.OrderByDescending(y => y.OlusturulmaTarihi).FirstOrDefault().Durak : null,
                YolcuNotlari = s.YolcuSayimlari.OrderByDescending(y => y.OlusturulmaTarihi).FirstOrDefault() != null ? s.YolcuSayimlari.OrderByDescending(y => y.OlusturulmaTarihi).FirstOrDefault().Notlar : null
            })
            .FirstOrDefaultAsync();

        if (sefer == null)
        {
            return NotFound();
        }

        return sefer;
    }

    // PUT: api/Seferler/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSefer(int id, SeferDto seferDto)
    {
        if (id != seferDto.Id)
        {
            return BadRequest();
        }

        var sefer = await _context.Seferler.FindAsync(id);
        if (sefer == null)
        {
            return NotFound();
        }

        sefer.HatId = seferDto.HatId;
        sefer.SoforId = seferDto.SoforId;
        sefer.OtobusId = seferDto.OtobusId;
        sefer.KalkisZamani = seferDto.KalkisZamani;
        sefer.VarisZamani = seferDto.VarisZamani;
        sefer.Durum = Enum.Parse<SeferDurum>(seferDto.Durum ?? "Planlandi");
        sefer.GidilenMesafeKm = seferDto.GidilenMesafeKm;
        sefer.GuncellenmeTarihi = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SeferExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Seferler
    [HttpPost]
    public async Task<ActionResult<Sefer>> PostSefer(SeferDto seferDto)
    {
        // Validate foreign keys
        var hatExists = await _context.Hatlar.AnyAsync(h => h.Id == seferDto.HatId);
        var soforExists = await _context.Kullanicilar.AnyAsync(k => k.Id == seferDto.SoforId);
        var otobusExists = await _context.Otobusler.AnyAsync(o => o.Id == seferDto.OtobusId);

        if (!hatExists)
        {
            return BadRequest("Geçersiz hat ID'si");
        }
        if (!soforExists)
        {
            return BadRequest("Geçersiz şoför ID'si");
        }
        if (!otobusExists)
        {
            return BadRequest("Geçersiz otobüs ID'si");
        }

        var sefer = new Sefer
        {
            HatId = seferDto.HatId,
            SoforId = seferDto.SoforId,
            OtobusId = seferDto.OtobusId,
            KalkisZamani = seferDto.KalkisZamani,
            VarisZamani = seferDto.VarisZamani,
            Durum = Enum.Parse<SeferDurum>(seferDto.Durum ?? "Planlandi"),
            GidilenMesafeKm = seferDto.GidilenMesafeKm,
            OlusturulmaTarihi = DateTime.Now
        };

        _context.Seferler.Add(sefer);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSefer", new { id = sefer.Id }, sefer);
    }

    // PUT: api/Seferler/5/durum
    [HttpPut("{id}/durum")]
    public async Task<IActionResult> UpdateSeferDurum(int id, [FromBody] UpdateDurumDto updateDurumDto)
    {
        var sefer = await _context.Seferler.FindAsync(id);
        if (sefer == null)
        {
            return NotFound();
        }

        // Enum değerini kontrol et
        if (!Enum.IsDefined(typeof(SeferDurum), updateDurumDto.Durum))
        {
            return BadRequest("Geçersiz durum değeri");
        }

        sefer.Durum = (SeferDurum)updateDurumDto.Durum;
        sefer.GuncellenmeTarihi = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SeferExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Seferler/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSefer(int id)
    {
        var sefer = await _context.Seferler.FindAsync(id);
        if (sefer == null)
        {
            return NotFound();
        }

        _context.Seferler.Remove(sefer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Seferler/{id}/yolcu-sayim
    [HttpPost("{id}/yolcu-sayim")]
    public async Task<ActionResult<YolcuSayim>> PostYolcuSayim(int id, YolcuSayimDto yolcuSayimDto)
    {
        // Sefer var mı kontrol et
        var seferExists = await _context.Seferler.AnyAsync(s => s.Id == id);
        if (!seferExists)
        {
            return NotFound("Sefer bulunamadı");
        }

        var yolcuSayim = new YolcuSayim
        {
            SeferId = id,
            YolcuSayisi = yolcuSayimDto.YolcuSayisi,
            Durak = yolcuSayimDto.Durak,
            Notlar = yolcuSayimDto.Notlar,
            OlusturulmaTarihi = DateTime.Now
        };

        _context.YolcuSayimlari.Add(yolcuSayim);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSefer", new { id = id }, yolcuSayim);
    }

    private bool SeferExists(int id)
    {
        return _context.Seferler.Any(e => e.Id == id);
    }
}

public class SeferDto
{
    public int Id { get; set; }
    public int HatId { get; set; }
    public int SoforId { get; set; }
    public int OtobusId { get; set; }
    public DateTime KalkisZamani { get; set; }
    public DateTime VarisZamani { get; set; }
    public string? Durum { get; set; }
    public double GidilenMesafeKm { get; set; }
}

public class YolcuSayimDto
{
    public int YolcuSayisi { get; set; }
    public string? Durak { get; set; }
    public string? Notlar { get; set; }
}

public class UpdateDurumDto
{
    public int Durum { get; set; }
}