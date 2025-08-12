namespace IettSeferSistemi.Domain.Entities;

public class Sefer : TemelEntity
{
    public int HatId { get; set; }
    public Hat Hat { get; set; } = null!;

    public int SoforId { get; set; }
    public Kullanici Sofor { get; set; } = null!;

    public int OtobusId { get; set; }
    public Otobus Otobus { get; set; } = null!;

    public DateTime KalkisZamani { get; set; }
    public DateTime VarisZamani { get; set; }

    public string Durum { get; set; } = "Planlandi"; // Planlandi, Tamamlandi, Iptal
    public double GidilenMesafeKm { get; set; } = 0;

    public ICollection<YolcuSayim> YolcuSayimlari { get; set; } = new List<YolcuSayim>();
}