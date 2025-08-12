namespace IettSeferSistemi.Domain.Entities;

public class BakimKaydi : TemelEntity
{
    public int OtobusId { get; set; }
    public Otobus Otobus { get; set; } = null!;
    public DateTime BakimTarihi { get; set; }
    public string Aciklama { get; set; } = null!;
    public decimal Maliyet { get; set; }
}