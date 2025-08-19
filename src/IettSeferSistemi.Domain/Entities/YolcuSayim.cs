namespace IettSeferSistemi.Domain.Entities;

public class YolcuSayim : TemelEntity
{
    public int SeferId { get; set; }
    public Sefer Sefer { get; set; } = null!;
    public int YolcuSayisi { get; set; }
    public string? Durak { get; set; }
    public string? Notlar { get; set; }
}