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

    public DbSet<CandidaturaEleccion> CandidaturasEleccion { get; set; }

    public DbSet<CodigoVerificacion> CodigosVerificacion { get; set; }

    public DbSet<Voto> Votos { get; set; }

    public DbSet<ParticipacionEleccion> ParticipacionesEleccion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ciudadano>()
            .HasIndex(c => c.NumeroDocumento)
            .IsUnique();

        modelBuilder.Entity<Ciudadano>()
            .HasIndex(c => c.CorreoElectronico)
            .IsUnique();

        modelBuilder.Entity<PartidoPolitico>()
            .HasIndex(p => p.Siglas)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.CorreoElectronico)
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

        modelBuilder.Entity<Candidato>()
            .HasOne(x => x.PartidoPolitico)
            .WithMany()
            .HasForeignKey(x => x.PartidoPoliticoId)
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<AsignacionCandidatoPuesto>(e =>
        {
            e.HasOne(x => x.PartidoPolitico).WithMany().HasForeignKey(x => x.PartidoPoliticoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Candidato).WithMany().HasForeignKey(x => x.CandidatoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PuestoElectivo).WithMany().HasForeignKey(x => x.PuestoElectivoId).OnDelete(DeleteBehavior.Restrict);

            // Un partido no puede tener dos candidatos en el mismo puesto,
            // ni el mismo candidato en dos puestos.
            e.HasIndex(x => new { x.PartidoPoliticoId, x.PuestoElectivoId }).IsUnique();
            e.HasIndex(x => new { x.PartidoPoliticoId, x.CandidatoId }).IsUnique();
        });

        modelBuilder.Entity<CandidaturaEleccion>(e =>
        {
            e.HasOne(x => x.Eleccion).WithMany().HasForeignKey(x => x.EleccionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PartidoPolitico).WithMany().HasForeignKey(x => x.PartidoPoliticoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Candidato).WithMany().HasForeignKey(x => x.CandidatoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PuestoElectivo).WithMany().HasForeignKey(x => x.PuestoElectivoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EleccionId, x.PartidoPoliticoId, x.PuestoElectivoId }).IsUnique();
        });

        modelBuilder.Entity<CodigoVerificacion>(e =>
        {
            e.HasOne(x => x.Ciudadano).WithMany().HasForeignKey(x => x.CiudadanoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Eleccion).WithMany().HasForeignKey(x => x.EleccionId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.CiudadanoId, x.EleccionId });
        });

        modelBuilder.Entity<Voto>(e =>
        {
            e.HasOne(x => x.Eleccion).WithMany().HasForeignKey(x => x.EleccionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PuestoElectivo).WithMany().HasForeignKey(x => x.PuestoElectivoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Candidato).WithMany().HasForeignKey(x => x.CandidatoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PartidoPolitico).WithMany().HasForeignKey(x => x.PartidoPoliticoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EleccionId, x.PuestoElectivoId });
        });

        modelBuilder.Entity<ParticipacionEleccion>(e =>
        {
            e.HasOne(x => x.Eleccion).WithMany().HasForeignKey(x => x.EleccionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Ciudadano).WithMany().HasForeignKey(x => x.CiudadanoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EleccionId, x.CiudadanoId }).IsUnique();
        });
    }
}
