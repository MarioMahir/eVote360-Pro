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

    public DbSet<PuestoElectivo> PuestosElectivos { get; set; }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<DirigentePolitico> DirigentesPoliticos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ciudadano>()
            .HasIndex(c => c.NumeroDocumento)
            .IsUnique();

        modelBuilder.Entity<Ciudadano>()
            .HasIndex(c => c.CorreoElectronico)
            .IsUnique();

        modelBuilder.Entity<DirigentePolitico>()
            .HasIndex(x => x.UsuarioId)
            .IsUnique();

        modelBuilder.Entity<DirigentePolitico>()
            .HasIndex(x => x.PartidoPoliticoId)
            .IsUnique();

        modelBuilder.Entity<DirigentePolitico>()
            .HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId);

        modelBuilder.Entity<DirigentePolitico>()
            .HasOne(x => x.PartidoPolitico)
            .WithMany()
            .HasForeignKey(x => x.PartidoPoliticoId);
    }
}