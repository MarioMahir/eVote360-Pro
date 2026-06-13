using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels;

public class UsuarioEditViewModel
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required]
    public string NombreUsuario { get; set; } = string.Empty;

    public string? Contrasena { get; set; }

    public string? ConfirmarContrasena { get; set; }

    [Required]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }
}