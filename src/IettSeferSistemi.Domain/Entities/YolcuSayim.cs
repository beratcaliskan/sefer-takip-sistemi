namespace IettSeferSistemi.Domain.Entities;

public class YolcuSayim : TemelEntity
{
    public int SeferId { get; set; }
    public Sefer Sefer { get; set; } = null!;
    public int YolcuSayisi { get; set; }
}