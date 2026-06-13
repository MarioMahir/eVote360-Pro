using System.Security.Cryptography;
using System.Text;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Core.DTOs.Usuarios;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<(bool Success, string Error)>CreateAsync(UsuarioCreateDto dto)
    {
        dto.NombreUsuario = dto.NombreUsuario.Trim();

        if (await _context.Usuarios.AnyAsync(x =>
            x.CorreoElectronico == dto.CorreoElectronico))
        {
            return (false,
                "Ya existe un usuario registrado con este correo electrónico.");
        }

        if (await _context.Usuarios.AnyAsync(x =>
            x.NombreUsuario == dto.NombreUsuario))
        {
            return (false,
                "Ya existe un usuario registrado con este nombre de usuario.");
        }

        if (dto.Contrasena != dto.ConfirmarContrasena)
        {
            return (false,
                "La contraseña y la confirmación de contraseña no coinciden.");
        }

        if (!EsRolValido(dto.Rol))
        {
            return (false,
                "Debe seleccionar un rol válido para el usuario.");
        }

        Usuario usuario = new()
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            CorreoElectronico = dto.CorreoElectronico,
            NombreUsuario = dto.NombreUsuario,
            PasswordHash = GenerarHash(dto.Contrasena),
            Rol = dto.Rol,
            Activo = dto.Activo
        };

        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(UsuarioUpdateDto model)
    {
        var usuario =
            await _context.Usuarios.FindAsync(model.Id);

        if (usuario == null)
            return (false, "Usuario no encontrado.");

        if (await _context.Usuarios.AnyAsync(x =>
            x.CorreoElectronico == model.CorreoElectronico &&
            x.Id != model.Id))
        {
            return (false,
                "Ya existe un usuario registrado con este correo electrónico.");
        }

        if (await _context.Usuarios.AnyAsync(x =>
            x.NombreUsuario == model.NombreUsuario &&
            x.Id != model.Id))
        {
            return (false,
                "Ya existe un usuario registrado con este nombre de usuario.");
        }

        if (!EsRolValido(model.Rol))
        {
            return (false,
                "Debe seleccionar un rol válido para el usuario.");
        }

        usuario.Nombre = model.Nombre;
        usuario.Apellido = model.Apellido;
        usuario.CorreoElectronico = model.CorreoElectronico;
        usuario.NombreUsuario = model.NombreUsuario.Trim();
        usuario.Rol = model.Rol;
        usuario.Activo = model.Activo;

        if (!string.IsNullOrWhiteSpace(model.Contrasena))
        {
            if (model.Contrasena != model.ConfirmarContrasena)
            {
                return (false,
                    "La contraseña y la confirmación de contraseña no coinciden.");
            }

            usuario.PasswordHash =
                GenerarHash(model.Contrasena);
        }

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        var usuario =
            await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return (false, "Usuario no encontrado.");

        if (usuario.Activo)
        {
            return (false,
                "Este usuario ya se encuentra activo.");
        }

        usuario.Activo = true;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id)
    {
        var usuario =
            await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return (false, "Usuario no encontrado.");

        if (!usuario.Activo)
        {
            return (false,
                "Este usuario ya se encuentra inactivo.");
        }

        var administradoresActivos =
            await _context.Usuarios.CountAsync(x =>
                x.Rol == "Administrador" &&
                x.Activo);

        if (usuario.Rol == "Administrador"
            && administradoresActivos == 1)
        {
            return (false,
                "No se puede desactivar este usuario porque es el único administrador activo del sistema.");
        }

        usuario.Activo = false;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    private static bool EsRolValido(string rol)
    {
        return rol == "Administrador"
            || rol == "Dirigente político";
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