using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Entities;

public class PuestoElectivo
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}