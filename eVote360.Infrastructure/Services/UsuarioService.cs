using System.Text.RegularExpressions;
using eVote360.Core.Constants;
using eVote360.Core.DTOs.Usuarios;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Repositories;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using eVote360.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IGenericRepository<Usuario> _usuarios;
    private readonly AppDbContext _context;
    private readonly IEleccionService _elecciones;

    public UsuarioService(
        IGenericRepository<Usuario> usuarios,
        AppDbContext context,
        IEleccionService elecciones)
    {
        _usuarios = usuarios;
        _context = context;
        _elecciones = elecciones;
    }

    public Task<List<Usuario>> GetAllAsync() =>
        _usuarios.Query().OrderBy(u => u.Apellido).ThenBy(u => u.Nombre).ToListAsync();

    public Task<Usuario?> GetByIdAsync(int id) => _usuarios.GetByIdAsync(id);

    public async Task<(bool Success, string Error)> CreateAsync(UsuarioCreateDto dto)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede crear un usuario mientras exista una elección activa.");

        dto.NombreUsuario = dto.NombreUsuario.Trim();
        dto.CorreoElectronico = dto.CorreoElectronico.Trim().ToLowerInvariant();

        if (await _usuarios.AnyAsync(x => x.CorreoElectronico == dto.CorreoElectronico))
            return (false, "Ya existe un usuario registrado con este correo electrónico.");

        if (await _usuarios.AnyAsync(x => x.NombreUsuario == dto.NombreUsuario))
            return (false, "Ya existe un usuario registrado con este nombre de usuario.");

        if (dto.Contrasena != dto.ConfirmarContrasena)
            return (false, "La contraseña y la confirmación de contraseña no coinciden.");

        var politica = ValidarPolitica(dto.Contrasena);
        if (politica != null) return (false, politica);

        if (!EsRolValido(dto.Rol))
            return (false, "Debe seleccionar un rol válido para el usuario.");

        await _usuarios.AddAsync(new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            CorreoElectronico = dto.CorreoElectronico,
            NombreUsuario = dto.NombreUsuario,
            PasswordHash = PasswordHasher.Hash(dto.Contrasena),
            Rol = dto.Rol,
            Activo = dto.Activo
        });

        await _usuarios.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(UsuarioUpdateDto dto, int usuarioActualId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede editar un usuario mientras exista una elección activa.");

        var usuario = await _usuarios.GetByIdAsync(dto.Id);

        if (usuario == null)
            return (false, "Usuario no encontrado.");

        dto.NombreUsuario = dto.NombreUsuario.Trim();
        dto.CorreoElectronico = dto.CorreoElectronico.Trim().ToLowerInvariant();

        if (await _usuarios.AnyAsync(x => x.CorreoElectronico == dto.CorreoElectronico && x.Id != dto.Id))
            return (false, "Ya existe un usuario registrado con este correo electrónico.");

        if (await _usuarios.AnyAsync(x => x.NombreUsuario == dto.NombreUsuario && x.Id != dto.Id))
            return (false, "Ya existe un usuario registrado con este nombre de usuario.");

        if (!EsRolValido(dto.Rol))
            return (false, "Debe seleccionar un rol válido para el usuario.");

        var cambiaRol = usuario.Rol != dto.Rol;
        var seDesactiva = usuario.Activo && !dto.Activo;

        if (dto.Id == usuarioActualId && (cambiaRol || seDesactiva))
            return (false, "No puede cambiar su propio rol ni desactivar su propio usuario mientras está autenticado.");

        if (cambiaRol && usuario.Rol == Roles.DirigentePolitico &&
            await _context.DirigentesPoliticos.AnyAsync(d => d.UsuarioId == usuario.Id))
        {
            return (false, "No se puede cambiar el rol de este usuario porque tiene un partido político asignado como dirigente.");
        }

        if ((cambiaRol || seDesactiva) && usuario.Rol == Roles.Administrador && await EsUnicoAdministradorActivoAsync(usuario.Id))
            return (false, "No se puede modificar este usuario porque es el único administrador activo del sistema.");

        if (!string.IsNullOrWhiteSpace(dto.Contrasena))
        {
            if (string.IsNullOrWhiteSpace(dto.ConfirmarContrasena))
                return (false, "Debe confirmar la nueva contraseña.");

            if (dto.Contrasena != dto.ConfirmarContrasena)
                return (false, "La contraseña y la confirmación de contraseña no coinciden.");

            var politica = ValidarPolitica(dto.Contrasena);
            if (politica != null) return (false, politica);

            usuario.PasswordHash = PasswordHasher.Hash(dto.Contrasena);
        }

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Apellido = dto.Apellido.Trim();
        usuario.CorreoElectronico = dto.CorreoElectronico;
        usuario.NombreUsuario = dto.NombreUsuario;
        usuario.Rol = dto.Rol;
        usuario.Activo = dto.Activo;

        await _usuarios.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede activar un usuario mientras exista una elección activa.");

        var usuario = await _usuarios.GetByIdAsync(id);

        if (usuario == null)
            return (false, "Usuario no encontrado.");

        if (usuario.Activo)
            return (false, "Este usuario ya se encuentra activo.");

        if (!EsRolValido(usuario.Rol))
            return (false, "El usuario no tiene un rol válido.");

        usuario.Activo = true;
        await _usuarios.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id, int usuarioActualId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede desactivar un usuario mientras exista una elección activa.");

        var usuario = await _usuarios.GetByIdAsync(id);

        if (usuario == null)
            return (false, "Usuario no encontrado.");

        if (!usuario.Activo)
            return (false, "Este usuario ya se encuentra inactivo.");

        if (id == usuarioActualId)
            return (false, "No puede desactivar su propio usuario mientras está autenticado.");

        if (usuario.Rol == Roles.Administrador && await EsUnicoAdministradorActivoAsync(id))
            return (false, "No se puede desactivar este usuario porque es el único administrador activo del sistema.");

        usuario.Activo = false;
        await _usuarios.SaveChangesAsync();

        return (true, string.Empty);
    }

    private async Task<bool> EsUnicoAdministradorActivoAsync(int usuarioId) =>
        !await _usuarios.AnyAsync(x => x.Rol == Roles.Administrador && x.Activo && x.Id != usuarioId);

    private static bool EsRolValido(string rol) => Roles.Todos.Contains(rol);

    private static string? ValidarPolitica(string contrasena)
    {
        if (contrasena.Length < 8)
            return "La contraseña debe tener al menos 8 caracteres.";

        if (!Regex.IsMatch(contrasena, "[A-Za-z]"))
            return "La contraseña debe contener al menos una letra.";

        if (!Regex.IsMatch(contrasena, "[0-9]"))
            return "La contraseña debe contener al menos un número.";

        return null;
    }
}
