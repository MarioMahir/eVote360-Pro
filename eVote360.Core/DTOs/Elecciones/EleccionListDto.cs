using eVote360.Core.Enums;

namespace eVote360.Core.DTOs.Elecciones;

public class EleccionListDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaEleccion { get; set; }

    public EstadoEleccion Estado { get; set; }

    public int PartidosParticipantes { get; set; }

    public int PuestosDisputados { get; set; }

    public int CiudadanosQueVotaron { get; set; }
}
