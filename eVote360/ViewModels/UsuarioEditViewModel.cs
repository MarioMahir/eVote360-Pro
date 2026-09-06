using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels;

public class UsuarioEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "El correo electrónico debe tener un formato válido.")]
    [MaxLength(150)]
    [Display(Name = "Correo electrónico")]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [MaxLength(50)]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña (opcional)")]
    public string? Contrasena { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Contrasena), ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarContrasena { get; set; }

    [Required(ErrorMessage = "El rol es requerido.")]
    public string Rol { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
