namespace eVote360.Core.Entities;

public class AlianzaPolitica
{
    public int Id { get; set; }

    public int PartidoSolicitanteId { get; set; }

    public PartidoPolitico PartidoSolicitante { get; set; } = null!;

    public int PartidoAliadoId { get; set; }

    public PartidoPolitico PartidoAliado { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; } = DateTime.Now;

    public DateTime? FechaRespuesta { get; set; }

    public EstadoAlianza Estado { get; set; } = EstadoAlianza.Pendiente;

    /// <summary>
    /// Una solicitud aceptada genera una alianza vigente. Al "eliminar" la alianza
    /// se marca como no vigente para conservar el histórico.
    /// </summary>
    public bool Vigente { get; set; }
}
