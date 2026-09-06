namespace eVote360.Core.Entities;

/// <summary>
/// Candidatura que participó en una elección: se registra al activarla a partir
/// de las asignaciones vigentes. Solo guarda relaciones (no copia datos), y sirve
/// para la boleta, los resultados y el bloqueo de campos críticos.
/// </summary>
public class CandidaturaEleccion
{
    public int Id { get; set; }

    public int EleccionId { get; set; }

    public Eleccion Eleccion { get; set; } = null!;

    public int PartidoPoliticoId { get; set; }

    public PartidoPolitico PartidoPolitico { get; set; } = null!;

    public int CandidatoId { get; set; }

    public Candidato Candidato { get; set; } = null!;

    public int PuestoElectivoId { get; set; }

    public PuestoElectivo PuestoElectivo { get; set; } = null!;
}
