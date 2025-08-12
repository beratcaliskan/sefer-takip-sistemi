namespace IettSeferSistemi.Domain.Entities;

public class SistemLog : TemelEntity
{
    public int? KullaniciId { get; set; } // bazı loglar sistem kaynaklı olabilir
    public Kullanici? Kullanici { get; set; }
    public string Islem { get; set; } = null!;
    public string Detay { get; set; } = null!;
    public DateTime Tarih { get; set; } = DateTime.Now;
}