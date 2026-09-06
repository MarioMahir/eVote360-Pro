namespace eVote360.Core.DTOs.Elecciones;

public class ResumenEleccionDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaEleccion { get; set; }

    public string Estado { get; set; } = string.Empty;

    public int PartidosParticipantes { get; set; }

    public int CandidatosParticipantes { get; set; }

    public int CiudadanosQueVotaron { get; set; }
}
