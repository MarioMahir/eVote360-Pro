using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Entities;

public class CodigoVerificacion
{
    public int Id { get; set; }

    public int CiudadanoId { get; set; }

    public Ciudadano Ciudadano { get; set; } = null!;

    public int EleccionId { get; set; }

    public Eleccion Eleccion { get; set; } = null!;

    [Required]
    [MaxLength(6)]
    public string Codigo { get; set; } = string.Empty;

    public DateTime FechaGeneracion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Usado { get; set; }
}
