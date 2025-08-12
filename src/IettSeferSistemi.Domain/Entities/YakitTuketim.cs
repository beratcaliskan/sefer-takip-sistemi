namespace IettSeferSistemi.Domain.Entities;

public class YakitTuketim : TemelEntity
{
    public int OtobusId { get; set; }
    public Otobus Otobus { get; set; } = null!;
    public DateTime Tarih { get; set; }
    public double Litre { get; set; }
    public decimal Tutar { get; set; }
}