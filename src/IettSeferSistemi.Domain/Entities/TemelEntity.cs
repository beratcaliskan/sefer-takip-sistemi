namespace IettSeferSistemi.Domain.Entities;

public abstract class TemelEntity
{
    public int Id { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    public DateTime? GuncellenmeTarihi { get; set; }
}