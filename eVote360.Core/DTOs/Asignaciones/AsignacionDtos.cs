namespace eVote360.Core.DTOs.Asignaciones;

public class AsignacionListDto
{
    public int Id { get; set; }

    public string CandidatoNombre { get; set; } = string.Empty;

    public string CandidatoApellido { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = string.Empty;

    public string PartidoOrigen { get; set; } = string.Empty;

    public string PartidoOrigenSiglas { get; set; } = string.Empty;

    public string PuestoElectivo { get; set; } = string.Empty;

    public bool EsAliado { get; set; }

    public string TipoCandidatura => EsAliado ? "Aliado" : "Propio";
}

public class CandidatoDisponibleDto
{
    public int Id { get; set; }

    public string Texto { get; set; } = string.Empty;

    public bool EsAliado { get; set; }
}

public class CandidatoListDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = string.Empty;

    public string? PuestoAsociado { get; set; }

    public bool Activo { get; set; }
}
