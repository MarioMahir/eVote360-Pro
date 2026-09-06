using System.ComponentModel.DataAnnotations;
using eVote360.Core.Enums;

namespace eVote360.Core.Entities;

public class Eleccion
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public DateTime FechaEleccion { get; set; }

    public EstadoEleccion Estado { get; set; } = EstadoEleccion.Pendiente;

    public DateTime? FechaActivacion { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
