using Microsoft.EntityFrameworkCore;
using eVote360.Core.Entities;

namespace eVote360.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ciudadano> Ciudadanos { get; set; }

    public DbSet<PartidoPolitico> PartidosPoliticos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ciudadano>()
            .HasIndex(c => c.NumeroDocumento)
            .IsUnique();

        modelBuilder.Entity<Ciudadano>()
            .HasIndex(c => c.CorreoElectronico)
            .IsUnique();
    }
}