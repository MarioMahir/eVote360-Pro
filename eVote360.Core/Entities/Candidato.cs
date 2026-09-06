using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Entities;

public class Candidato
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    public string FotoUrl { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public int PartidoPoliticoId { get; set; }

    public PartidoPolitico PartidoPolitico { get; set; } = null!;

    public string NombreCompleto => $"{Nombre} {Apellido}";
}
