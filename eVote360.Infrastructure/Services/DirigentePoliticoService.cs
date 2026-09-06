using eVote360.Core.Constants;
using eVote360.Core.DTOs.DirigentesPoliticos;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class DirigentePoliticoService : IDirigentePoliticoService
{
    private readonly AppDbContext _context;
    private readonly IEleccionService _elecciones;

    public DirigentePoliticoService(AppDbContext context, IEleccionService elecciones)
    {
        _context = context;
        _elecciones = elecciones;
    }

    public Task<List<DirigentePolitico>> GetAllAsync() =>
        _context.DirigentesPoliticos
            .Include(x => x.Usuario)
            .Include(x => x.PartidoPolitico)
            .OrderBy(x => x.PartidoPolitico.Nombre)
            .ToListAsync();

    public async Task<List<Usuario>> GetDirigentesDisponiblesAsync()
    {
        var asignados = await _context.DirigentesPoliticos.Select(x => x.UsuarioId).ToListAsync();

        return await _context.Usuarios
            .Where(x => x.Activo && x.Rol == Roles.DirigentePolitico && !asignados.Contains(x.Id))
            .OrderBy(x => x.Apellido)
            .ToListAsync();
    }

    public async Task<List<PartidoPolitico>> GetPartidosDisponiblesAsync()
    {
        var asignados = await _context.DirigentesPoliticos.Select(x => x.PartidoPoliticoId).ToListAsync();

        return await _context.PartidosPoliticos
            .Where(x => x.Activo && !asignados.Contains(x.Id))
            .OrderBy(x => x.Nombre)
            .ToListAsync();
    }

    public Task<DirigentePolitico?> GetByIdAsync(int id) =>
        _context.DirigentesPoliticos
            .Include(x => x.Usuario)
            .Include(x => x.PartidoPolitico)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<(bool Success, string Error)> CreateAsync(DirigentePoliticoCreateDto dto)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede crear una asignación de dirigente político mientras exista una elección activa.");

        var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);

        if (usuario == null)
            return (false, "El usuario seleccionado no existe.");

        if (!usuario.Activo)
            return (false, "El usuario seleccionado debe estar activo.");

        if (usuario.Rol != Roles.DirigentePolitico)
            return (false, "El usuario seleccionado no tiene el rol de dirigente político.");

        if (await _context.DirigentesPoliticos.AnyAsync(x => x.UsuarioId == dto.UsuarioId))
            return (false, "Este dirigente ya está relacionado con otro partido político.");

        var partido = await _context.PartidosPoliticos.FindAsync(dto.PartidoPoliticoId);

        if (partido == null)
            return (false, "El partido político seleccionado no existe.");

        if (!partido.Activo)
            return (false, "El partido político seleccionado debe estar activo.");

        if (await _context.DirigentesPoliticos.AnyAsync(x => x.PartidoPoliticoId == dto.PartidoPoliticoId))
            return (false, "Este partido político ya tiene un dirigente asignado.");

        _context.DirigentesPoliticos.Add(new DirigentePolitico
        {
            UsuarioId = dto.UsuarioId,
            PartidoPoliticoId = dto.PartidoPoliticoId
        });

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede eliminar una asignación de dirigente político mientras exista una elección activa.");

        var relacion = await _context.DirigentesPoliticos.FindAsync(id);

        if (relacion == null)
            return (false, "La asignación seleccionada no existe o ya fue eliminada.");

        _context.DirigentesPoliticos.Remove(relacion);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}
