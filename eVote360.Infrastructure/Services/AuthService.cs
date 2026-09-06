using eVote360.Core.Constants;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using eVote360.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Error, Usuario? Usuario)> LoginAsync(string nombreUsuario, string password)
    {
        nombreUsuario = (nombreUsuario ?? string.Empty).Trim();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(x => x.NombreUsuario == nombreUsuario);

        if (usuario == null || !PasswordHasher.Verify(password ?? string.Empty, usuario.PasswordHash))
            return (false, "Los datos de acceso son inválidos.", null);

        if (!usuario.Activo)
            return (false, "El usuario está inactivo.", null);

        if (!Roles.Todos.Contains(usuario.Rol))
            return (false, "El usuario no tiene un rol válido dentro del sistema.", null);

        if (usuario.Rol == Roles.DirigentePolitico)
        {
            var asignacion = await _context.DirigentesPoliticos
                .Include(x => x.PartidoPolitico)
                .FirstOrDefaultAsync(x => x.UsuarioId == usuario.Id);

            if (asignacion == null)
                return (false, "No tiene un partido político asignado, por lo tanto no puede iniciar sesión. Por favor, póngase en contacto con un administrador.", null);

            if (!asignacion.PartidoPolitico.Activo)
                return (false, "El partido político asignado a este usuario se encuentra inactivo.", null);
        }

        return (true, string.Empty, usuario);
    }
}
