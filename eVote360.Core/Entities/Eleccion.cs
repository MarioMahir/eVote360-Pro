using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Entities;

public class Eleccion
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public DateTime FechaEleccion { get; set; }

    public bool Activa { get; set; }

    public bool Finalizada { get; set; }
}