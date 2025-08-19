namespace IettSeferSistemi.Domain.Entities;

public class Hat : TemelEntity
{
    public string HatKodu { get; set; } = null!; // Ör: "34BZ"
    public string BaslangicDuragi { get; set; } = null!;
    public string BitisDuragi { get; set; } = null!;
    public double MesafeKm { get; set; }
    public int TahminSureDakika { get; set; }
    public int DurakSayisi { get; set; }
    public string? Aciklama { get; set; }
    public bool Durum { get; set; } = true; // true = aktif

    public ICollection<Sefer> Seferler { get; set; } = new List<Sefer>();
}