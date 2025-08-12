using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IettSeferSistemi.Infrastructure.Data;
using IettSeferSistemi.Domain.Entities;

namespace IettSeferSistemi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HatlarController : ControllerBase
{
    private readonly IettDbContext _context;

    public HatlarController(IettDbContext context)
    {
        _context = context;
    }

    // GET: api/Hatlar
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hat>>> GetHatlar()
    {
        return await _context.Hatlar.ToListAsync();
    }

    // GET: api/Hatlar/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Hat>> GetHat(int id)
    {
        var hat = await _context.Hatlar.FindAsync(id);

        if (hat == null)
        {
            return NotFound();
        }

        return hat;
    }

    // PUT: api/Hatlar/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHat(int id, Hat hat)
    {
        if (id != hat.Id)
        {
            return BadRequest();
        }

        _context.Entry(hat).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HatExists(id))
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

    // POST: api/Hatlar
    [HttpPost]
    public async Task<ActionResult<Hat>> PostHat(Hat hat)
    {
        _context.Hatlar.Add(hat);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetHat", new { id = hat.Id }, hat);
    }

    // DELETE: api/Hatlar/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHat(int id)
    {
        var hat = await _context.Hatlar.FindAsync(id);
        if (hat == null)
        {
            return NotFound();
        }

        _context.Hatlar.Remove(hat);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool HatExists(int id)
    {
        return _context.Hatlar.Any(e => e.Id == id);
    }
}