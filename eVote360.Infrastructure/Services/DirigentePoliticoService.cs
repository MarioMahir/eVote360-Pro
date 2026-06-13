using eVote360.Core.DTOs.DirigentesPoliticos;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class DirigentePoliticoService : IDirigentePoliticoService
{
    private readonly AppDbContext _context;

    public DirigentePoliticoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DirigentePolitico>> GetAllAsync()
    {
        return await _context.DirigentesPoliticos
            .Include(x => x.Usuario)
            .Include(x => x.PartidoPolitico)
            .ToListAsync();
    }

    public async Task<List<Usuario>> GetDirigentesDisponiblesAsync()
    {
        var asignados =
            await _context.DirigentesPoliticos
                .Select(x => x.UsuarioId)
                .ToListAsync();

        return await _context.Usuarios
            .Where(x =>
                x.Activo &&
                x.Rol == "Dirigente político" &&
                !asignados.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<List<PartidoPolitico>> GetPartidosDisponiblesAsync()
    {
        var asignados =
            await _context.DirigentesPoliticos
                .Select(x => x.PartidoPoliticoId)
                .ToListAsync();

        return await _context.PartidosPoliticos
            .Where(x =>
                x.Activo &&
                !asignados.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<DirigentePolitico?> GetByIdAsync(int id)
    {
        return await _context.DirigentesPoliticos
            .Include(x => x.Usuario)
            .Include(x => x.PartidoPolitico)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(bool Success, string Error)> CreateAsync(
        DirigentePoliticoCreateDto dto)
    {
        var usuario =
            await _context.Usuarios
                .FindAsync(dto.UsuarioId);

        if (usuario == null)
            return (false, "El usuario seleccionado no existe.");

        if (!usuario.Activo)
            return (false, "El usuario debe estar activo.");

        if (usuario.Rol != "Dirigente político")
            return (false,
                "El usuario seleccionado no tiene el rol de dirigente político.");

        bool usuarioAsignado =
            await _context.DirigentesPoliticos
                .AnyAsync(x => x.UsuarioId == dto.UsuarioId);

        if (usuarioAsignado)
            return (false,
                "Este dirigente ya está relacionado con otro partido político.");

        var partido =
            await _context.PartidosPoliticos
                .FindAsync(dto.PartidoPoliticoId);

        if (partido == null)
            return (false, "El partido político no existe.");

        if (!partido.Activo)
            return (false, "El partido político debe estar activo.");

        bool partidoAsignado =
            await _context.DirigentesPoliticos
                .AnyAsync(x =>
                    x.PartidoPoliticoId ==
                    dto.PartidoPoliticoId);

        if (partidoAsignado)
            return (false,
                "Este partido político ya tiene un dirigente asignado.");

        _context.DirigentesPoliticos.Add(
            new DirigentePolitico
            {
                UsuarioId = dto.UsuarioId,
                PartidoPoliticoId = dto.PartidoPoliticoId
            });

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteAsync(int id)
    {
        var relacion =
            await _context.DirigentesPoliticos
                .FindAsync(id);

        if (relacion == null)
        {
            return (false,
                "La asignación seleccionada no existe o ya fue eliminada.");
        }

        _context.DirigentesPoliticos.Remove(relacion);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}