namespace eVote360.Core.Entities;

public class AsignacionCandidatoPuesto
{
    public int Id { get; set; }

    public int EleccionId { get; set; }

    public Eleccion Eleccion { get; set; } = null!;

    public int CandidatoId { get; set; }

    public Candidato Candidato { get; set; } = null!;

    public int PuestoElectivoId { get; set; }

    public PuestoElectivo PuestoElectivo { get; set; } = null!;

    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    public bool Activo { get; set; } = true;
}