namespace eVote360.Core.Entities;

public class AlianzaPolitica
{
    public int Id { get; set; }

    public int PartidoSolicitanteId { get; set; }

    public PartidoPolitico PartidoSolicitante { get; set; } = null!;

    public int PartidoAliadoId { get; set; }

    public PartidoPolitico PartidoAliado { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; } = DateTime.Now;

    public EstadoAlianza Estado { get; set; } = EstadoAlianza.Pendiente;
}