using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Entities;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}