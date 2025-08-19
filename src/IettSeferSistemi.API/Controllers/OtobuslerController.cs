using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IettSeferSistemi.Infrastructure.Data;
using IettSeferSistemi.Domain.Entities;

namespace IettSeferSistemi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtobuslerController : ControllerBase
{
    private readonly IettDbContext _context;

    public OtobuslerController(IettDbContext context)
    {
        _context = context;
    }

    // GET: api/Otobusler
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Otobus>>> GetOtobusler()
    {
        return await _context.Otobusler.ToListAsync();
    }

    // GET: api/Otobusler/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Otobus>> GetOtobus(int id)
    {
        var otobus = await _context.Otobusler.FindAsync(id);

        if (otobus == null)
        {
            return NotFound();
        }

        return otobus;
    }

    // PUT: api/Otobusler/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutOtobus(int id, Otobus otobus)
    {
        if (id != otobus.Id)
        {
            return BadRequest();
        }

        otobus.GuncellenmeTarihi = DateTime.Now;
        _context.Entry(otobus).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OtobusExists(id))
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

    // POST: api/Otobusler
    [HttpPost]
    public async Task<ActionResult<Otobus>> PostOtobus(Otobus otobus)
    {
        otobus.OlusturulmaTarihi = DateTime.Now;
        _context.Otobusler.Add(otobus);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetOtobus", new { id = otobus.Id }, otobus);
    }

    // DELETE: api/Otobusler/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOtobus(int id)
    {
        var otobus = await _context.Otobusler.FindAsync(id);
        if (otobus == null)
        {
            return NotFound();
        }

        _context.Otobusler.Remove(otobus);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OtobusExists(int id)
    {
        return _context.Otobusler.Any(e => e.Id == id);
    }
}