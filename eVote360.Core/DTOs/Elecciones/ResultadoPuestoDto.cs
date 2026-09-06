namespace eVote360.Core.DTOs.Elecciones;

public class ResultadoPuestoDto
{
    public int PuestoElectivoId { get; set; }

    public string PuestoNombre { get; set; } = string.Empty;

    public int TotalVotos { get; set; }

    public bool Empate { get; set; }

    public List<OpcionResultadoDto> Opciones { get; set; } = [];
}

public class OpcionResultadoDto
{
    public int? CandidatoId { get; set; }

    public string Candidato { get; set; } = string.Empty;

    public string? FotoUrl { get; set; }

    public string Partido { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public int Votos { get; set; }

    public decimal Porcentaje { get; set; }

    /// <summary>Ganador único del puesto.</summary>
    public bool Ganador { get; set; }

    /// <summary>Comparte el primer lugar en caso de empate.</summary>
    public bool EmpatePrimerLugar { get; set; }
}
