namespace eVote360.Core.Entities;

/// <summary>Marca que un ciudadano finalizó su votación en una elección.</summary>
public class ParticipacionEleccion
{
    public int Id { get; set; }

    public int EleccionId { get; set; }

    public Eleccion Eleccion { get; set; } = null!;

    public int CiudadanoId { get; set; }

    public Ciudadano Ciudadano { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.Now;
}
