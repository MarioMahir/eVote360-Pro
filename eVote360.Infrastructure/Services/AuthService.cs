using System.Security.Cryptography;
using System.Text;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Error, Usuario? Usuario)>
    LoginAsync(
        string nombreUsuario,
        string password)
    {
        string hash = GenerarHash(password);

        var usuario =
            await _context.Usuarios
            .FirstOrDefaultAsync(x =>
                x.NombreUsuario == nombreUsuario);

        if (usuario == null)
        {
            return (false,
                "Usuario o contraseña incorrectos.",
                null);
        }

        if (!usuario.Activo)
        {
            return (false,
                "El usuario se encuentra inactivo.",
                null);
        }

        if (usuario.PasswordHash != hash)
        {
            return (false,
                "Usuario o contraseña incorrectos.",
                null);
        }

        if (usuario.Rol == "Dirigente político")
        {
            var asignacion =
                await _context.DirigentesPoliticos
                .Include(x => x.PartidoPolitico)
                .FirstOrDefaultAsync(x =>
                    x.UsuarioId == usuario.Id);

            if (asignacion == null)
            {
                return (false,
                    "No tiene un partido político asignado. Por favor, póngase en contacto con un administrador.",
                    null);
            }

            if (!asignacion.PartidoPolitico.Activo)
            {
                return (false,
                    "El partido político asignado a este usuario se encuentra inactivo.",
                    null);
            }
        }

        return (true, string.Empty, usuario);
    }

    private static string GenerarHash(string texto)
    {
        using var sha256 = SHA256.Create();

        var bytes =
            sha256.ComputeHash(
                Encoding.UTF8.GetBytes(texto));

        return Convert.ToBase64String(bytes);
    }
}