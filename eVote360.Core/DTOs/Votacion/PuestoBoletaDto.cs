namespace eVote360.Core.DTOs.Votacion;

public class PuestoBoletaDto
{
    public int PuestoElectivoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public int PartidosParticipantes { get; set; }

    public int CandidatosReales { get; set; }

    public bool Seleccionado { get; set; }

    public string? SeleccionTexto { get; set; }
}

public class OpcionBoletaDto
{
    public int CandidatoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = string.Empty;

    public int PartidoPoliticoId { get; set; }

    public string PartidoNombre { get; set; } = string.Empty;

    public string PartidoSiglas { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;
}

/// <summary>Selección del elector para un puesto. CandidatoId nulo = "Ninguno".</summary>
public class SeleccionVotoDto
{
    public int PuestoElectivoId { get; set; }

    public int? CandidatoId { get; set; }

    public int? PartidoPoliticoId { get; set; }
}

public class ResumenVotacionDto
{
    public string CiudadanoNombre { get; set; } = string.Empty;

    public string CorreoElectronico { get; set; } = string.Empty;

    public string EleccionNombre { get; set; } = string.Empty;

    public DateTime FechaEleccion { get; set; }

    public List<ResumenVotacionItemDto> Selecciones { get; set; } = [];
}

public class ResumenVotacionItemDto
{
    public string Puesto { get; set; } = string.Empty;

    public string Seleccion { get; set; } = string.Empty;

    public string? Partido { get; set; }
}
