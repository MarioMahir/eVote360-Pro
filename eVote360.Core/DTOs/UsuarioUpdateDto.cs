using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.DTOs.Usuarios;

public class UsuarioUpdateDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string CorreoElectronico { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string? Contrasena { get; set; }

    public string? ConfirmarContrasena { get; set; }

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }
}