using eVote360.Core.Entities;

namespace eVote360.Core.DTOs.Alianzas;

public class SolicitudAlianzaDto
{
    public int Id { get; set; }

    public int PartidoId { get; set; }

    public string PartidoNombre { get; set; } = string.Empty;

    public string PartidoSiglas { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public EstadoAlianza Estado { get; set; }

    public string EstadoTexto => Estado switch
    {
        EstadoAlianza.Pendiente => "En espera de respuesta",
        EstadoAlianza.Aceptada => "Aceptada",
        EstadoAlianza.Rechazada => "Rechazada",
        _ => Estado.ToString()
    };

    public bool PuedeEliminar => Estado != EstadoAlianza.Aceptada;
}

public class AlianzaVigenteDto
{
    public int Id { get; set; }

    public int PartidoAliadoId { get; set; }

    public string PartidoAliadoNombre { get; set; } = string.Empty;

    public string PartidoAliadoSiglas { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public DateTime FechaAceptacion { get; set; }
}
