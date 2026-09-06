namespace eVote360.Core.Entities;

/// <summary>
/// Voto emitido para un puesto dentro de una elección. No guarda el ciudadano
/// para preservar la confidencialidad del voto: la participación se registra
/// aparte en <see cref="ParticipacionEleccion"/>.
/// CandidatoId nulo representa la opción "Ninguno".
/// </summary>
public class Voto
{
    public int Id { get; set; }

    public int EleccionId { get; set; }

    public Eleccion Eleccion { get; set; } = null!;

    public int PuestoElectivoId { get; set; }

    public PuestoElectivo PuestoElectivo { get; set; } = null!;

    public int? CandidatoId { get; set; }

    public Candidato? Candidato { get; set; }

    public int? PartidoPoliticoId { get; set; }

    public PartidoPolitico? PartidoPolitico { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;
}
