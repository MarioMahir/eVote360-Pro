using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.DTOs.Auth;

public class LoginDto
{
    [Required]
    public string NombreUsuario { get; set; }
        = string.Empty;

    [Required]
    public string Contrasena { get; set; }
        = string.Empty;
}