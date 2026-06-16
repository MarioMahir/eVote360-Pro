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

    public DbSet<Candidato> Candidatos { get; set; }




    public DbSet<Eleccion> Elecciones { get; set; }

    public DbSet<AlianzaPolitica> AlianzasPoliticas { get; set; }

    public DbSet<AsignacionCandidatoPuesto> AsignacionesCandidatoPuesto { get; set; }

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


        modelBuilder.Entity<AlianzaPolitica>()
            .HasOne(x => x.PartidoSolicitante)
            .WithMany()
            .HasForeignKey(x => x.PartidoSolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AlianzaPolitica>()
            .HasOne(x => x.PartidoAliado)
            .WithMany()
            .HasForeignKey(x => x.PartidoAliadoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsignacionCandidatoPuesto>()
            .HasOne(x => x.Eleccion)
            .WithMany()
            .HasForeignKey(x => x.EleccionId);

        modelBuilder.Entity<AsignacionCandidatoPuesto>()
            .HasOne(x => x.Candidato)
            .WithMany()
            .HasForeignKey(x => x.CandidatoId);

        modelBuilder.Entity<AsignacionCandidatoPuesto>()
            .HasOne(x => x.PuestoElectivo)
            .WithMany()
            .HasForeignKey(x => x.PuestoElectivoId);
    }
}