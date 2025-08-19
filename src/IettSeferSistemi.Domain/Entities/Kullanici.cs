namespace IettSeferSistemi.Domain.Entities;

public class Kullanici : TemelEntity
{
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string AdSoyad => $"{Ad} {Soyad}";
    public string KullaniciAdi { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Sifre { get; set; } = null!; // Düz metin olarak saklanacak (önerilmez)
    public string Rol { get; set; } = "Kullanici"; // Sofor, Yonetici, Admin vb.
    public bool Active { get; set; } = true; // Kullanıcının aktif/pasif durumu

    public ICollection<Sefer> Seferler { get; set; } = new List<Sefer>();
    public ICollection<SistemLog> Loglar { get; set; } = new List<SistemLog>();
}