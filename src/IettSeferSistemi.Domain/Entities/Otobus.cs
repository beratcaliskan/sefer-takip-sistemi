namespace IettSeferSistemi.Domain.Entities;

public class Otobus : TemelEntity
{
    public string Plaka { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Kapasite { get; set; }

    public ICollection<Sefer> Seferler { get; set; } = new List<Sefer>();
    public ICollection<BakimKaydi> BakimKayitlari { get; set; } = new List<BakimKaydi>();
    public ICollection<YakitTuketim> YakitTuketimleri { get; set; } = new List<YakitTuketim>();
}