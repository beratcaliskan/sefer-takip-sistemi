namespace IettSeferSistemi.Domain.Entities;

public class Hat : TemelEntity
{
    public string HatKodu { get; set; } = null!; // Ör: "34BZ"
    public string HatAdi { get; set; } = null!;
    public bool Durum { get; set; } = true; // true = aktif

    public ICollection<Sefer> Seferler { get; set; } = new List<Sefer>();
}