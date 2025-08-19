using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IettSeferSistemi.Infrastructure.Data;
using IettSeferSistemi.Domain.Entities;

namespace IettSeferSistemi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KullanicilarController : ControllerBase
{
    private readonly IettDbContext _context;

    public KullanicilarController(IettDbContext context)
    {
        _context = context;
    }

    // POST: api/Kullanicilar/login
    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(LoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
        {
            return BadRequest(new { success = false, message = "Kullanıcı adı ve şifre gereklidir." });
        }

        try
        {
            // Kullanıcıyı KullaniciAdi ile ara
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi == loginDto.Username && k.Sifre == loginDto.Password);

            if (kullanici == null)
            {
                return Unauthorized(new { success = false, message = "Kullanıcı adı veya şifre hatalı!" });
            }

            // Başarılı giriş
            var result = new {
                success = true,
                message = "Giriş başarılı!",
                user = new {
                    kullanici.Id,
                    Name = kullanici.Ad + " " + kullanici.Soyad,
                    kullanici.Email,
                    Username = kullanici.KullaniciAdi,
                    Role = kullanici.Rol.ToLower()
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Giriş işlemi sırasında bir hata oluştu." });
        }
    }

    // GET: api/Kullanicilar
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetKullanicilar()
    {
        var kullanicilar = await _context.Kullanicilar.ToListAsync();
        
        var result = kullanicilar.Select(k => new {
            k.Id,
            k.Ad,
            k.Soyad,
            Name = k.Ad + " " + k.Soyad,
            k.Email,
            Username = k.KullaniciAdi,
            Role = k.Rol.ToLower(),
            k.Active,
            LastLogin = k.GuncellenmeTarihi?.ToString("yyyy-MM-dd HH:mm") ?? "Henüz giriş yapmadı",
            k.OlusturulmaTarihi,
            k.GuncellenmeTarihi
        }).ToList();

        return Ok(result);
    }

    // GET: api/Kullanicilar/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetKullanici(int id)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(id);

        if (kullanici == null)
        {
            return NotFound();
        }

        var result = new {
            kullanici.Id,
            kullanici.Ad,
            kullanici.Soyad,
            Name = kullanici.Ad + " " + kullanici.Soyad,
            kullanici.Email,
            Username = kullanici.KullaniciAdi,
            Role = kullanici.Rol.ToLower(),
            kullanici.Active,
            LastLogin = kullanici.GuncellenmeTarihi?.ToString("yyyy-MM-dd HH:mm") ?? "Henüz giriş yapmadı",
            kullanici.OlusturulmaTarihi,
            kullanici.GuncellenmeTarihi
        };

        return Ok(result);
    }

    // PUT: api/Kullanicilar/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutKullanici(int id, KullaniciUpdateDto kullaniciDto)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici == null)
        {
            return NotFound();
        }

        // Ad ve Soyad'ı name'den ayır
        var nameParts = kullaniciDto.Name.Split(' ', 2);
        kullanici.Ad = nameParts[0];
        kullanici.Soyad = nameParts.Length > 1 ? nameParts[1] : "";
        kullanici.KullaniciAdi = kullaniciDto.Username;
        kullanici.Email = kullaniciDto.Email;
        kullanici.Rol = kullaniciDto.Role;
        kullanici.Active = kullaniciDto.Active;
        
        // Şifre varsa güncelle
        if (!string.IsNullOrEmpty(kullaniciDto.Password))
        {
            kullanici.Sifre = kullaniciDto.Password;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!KullaniciExists(id))
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

    // POST: api/Kullanicilar/update-usernames
    [HttpPost("update-usernames")]
    public async Task<ActionResult<object>> UpdateUsernames()
    {
        try
        {
            // Boş KullaniciAdi olan kullanıcıları bul
            var kullanicilar = await _context.Kullanicilar
                .Where(k => string.IsNullOrEmpty(k.KullaniciAdi))
                .ToListAsync();

            int updatedCount = 0;
            foreach (var kullanici in kullanicilar)
            {
                if (!string.IsNullOrEmpty(kullanici.Email))
                {
                    // Email'in @ öncesi kısmını kullanıcı adı olarak ata
                    var emailParts = kullanici.Email.Split('@');
                    if (emailParts.Length > 0)
                    {
                        kullanici.KullaniciAdi = emailParts[0];
                        updatedCount++;
                    }
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new { 
                success = true, 
                message = $"{updatedCount} kullanıcının kullanıcı adı güncellendi.",
                updatedCount = updatedCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                success = false, 
                message = "Kullanıcı adları güncellenirken hata oluştu." 
            });
        }
    }

    // POST: api/Kullanicilar
    [HttpPost]
    public async Task<ActionResult<object>> PostKullanici(KullaniciCreateDto kullaniciDto)
    {
        // Ad ve Soyad'ı name'den ayır
        var nameParts = kullaniciDto.Name.Split(' ', 2);
        
        var kullanici = new Kullanici
        {
            Ad = nameParts[0],
            Soyad = nameParts.Length > 1 ? nameParts[1] : "",
            KullaniciAdi = kullaniciDto.Username,
            Email = kullaniciDto.Email,
            Sifre = kullaniciDto.Password,
            Rol = kullaniciDto.Role,
            Active = kullaniciDto.Active
        };

        _context.Kullanicilar.Add(kullanici);
        await _context.SaveChangesAsync();

        var result = new {
            kullanici.Id,
            kullanici.Ad,
            kullanici.Soyad,
            Name = kullanici.Ad + " " + kullanici.Soyad,
            kullanici.Email,
            Username = kullanici.Email.Split('@')[0],
            Role = kullanici.Rol.ToLower(),
            Active = true,
            LastLogin = "Henüz giriş yapmadı",
            kullanici.OlusturulmaTarihi,
            kullanici.GuncellenmeTarihi
        };

        return CreatedAtAction("GetKullanici", new { id = kullanici.Id }, result);
    }

    // DELETE: api/Kullanicilar/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteKullanici(int id)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici == null)
        {
            return NotFound();
        }

        _context.Kullanicilar.Remove(kullanici);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool KullaniciExists(int id)
    {
        return _context.Kullanicilar.Any(e => e.Id == id);
    }
}

// DTO sınıfları
public class KullaniciCreateDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool Active { get; set; } = true;
}

public class KullaniciUpdateDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? Password { get; set; }
    public string Role { get; set; } = null!;
    public bool Active { get; set; } = true;
}

public class LoginDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}