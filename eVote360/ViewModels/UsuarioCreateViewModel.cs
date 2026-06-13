using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels;

public class UsuarioCreateViewModel
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Contrasena { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Contrasena))]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}