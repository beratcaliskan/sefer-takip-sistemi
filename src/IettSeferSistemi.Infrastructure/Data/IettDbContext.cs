using Microsoft.EntityFrameworkCore;
using IettSeferSistemi.Domain.Entities;

namespace IettSeferSistemi.Infrastructure.Data;

public class IettDbContext : DbContext
{
    public IettDbContext(DbContextOptions<IettDbContext> options) : base(options)
    {
    }

    public DbSet<Kullanici> Kullanicilar { get; set; }
    public DbSet<Hat> Hatlar { get; set; }
    public DbSet<Otobus> Otobusler { get; set; }
    public DbSet<Sefer> Seferler { get; set; }
    public DbSet<BakimKaydi> BakimKayitlari { get; set; }
    public DbSet<YakitTuketim> YakitTuketimleri { get; set; }
    public DbSet<YolcuSayim> YolcuSayimlari { get; set; }
    public DbSet<SistemLog> SistemLoglari { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kullanici - Sefer ilişkisi (Şoför)
        modelBuilder.Entity<Sefer>()
            .HasOne(s => s.Sofor)
            .WithMany(k => k.Seferler)
            .HasForeignKey(s => s.SoforId)
            .OnDelete(DeleteBehavior.Restrict);

        // Hat - Sefer ilişkisi
        modelBuilder.Entity<Sefer>()
            .HasOne(s => s.Hat)
            .WithMany(h => h.Seferler)
            .HasForeignKey(s => s.HatId)
            .OnDelete(DeleteBehavior.Restrict);

        // Otobus - Sefer ilişkisi
        modelBuilder.Entity<Sefer>()
            .HasOne(s => s.Otobus)
            .WithMany(o => o.Seferler)
            .HasForeignKey(s => s.OtobusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Otobus - BakimKaydi ilişkisi
        modelBuilder.Entity<BakimKaydi>()
            .HasOne(b => b.Otobus)
            .WithMany(o => o.BakimKayitlari)
            .HasForeignKey(b => b.OtobusId)
            .OnDelete(DeleteBehavior.Cascade);

        // Otobus - YakitTuketim ilişkisi
        modelBuilder.Entity<YakitTuketim>()
            .HasOne(y => y.Otobus)
            .WithMany(o => o.YakitTuketimleri)
            .HasForeignKey(y => y.OtobusId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sefer - YolcuSayim ilişkisi
        modelBuilder.Entity<YolcuSayim>()
            .HasOne(y => y.Sefer)
            .WithMany(s => s.YolcuSayimlari)
            .HasForeignKey(y => y.SeferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kullanici - SistemLog ilişkisi (opsiyonel)
        modelBuilder.Entity<SistemLog>()
            .HasOne(s => s.Kullanici)
            .WithMany(k => k.Loglar)
            .HasForeignKey(s => s.KullaniciId)
            .OnDelete(DeleteBehavior.SetNull);

        // Decimal precision ayarları
        modelBuilder.Entity<BakimKaydi>()
            .Property(b => b.Maliyet)
            .HasPrecision(18, 2);

        modelBuilder.Entity<YakitTuketim>()
            .Property(y => y.Tutar)
            .HasPrecision(18, 2);
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries<TemelEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.OlusturulmaTarihi = DateTime.UtcNow;

            if (entry.State == EntityState.Modified)
                entry.Entity.GuncellenmeTarihi = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<TemelEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.OlusturulmaTarihi = DateTime.UtcNow;

            if (entry.State == EntityState.Modified)
                entry.Entity.GuncellenmeTarihi = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}