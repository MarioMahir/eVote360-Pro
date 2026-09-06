using eVote360.Core.DTOs.Dirigente;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class DirigenteService : IDirigenteService
{
    private readonly AppDbContext _context;

    public DirigenteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PartidoPolitico?> GetPartidoDeUsuarioAsync(int usuarioId)
    {
        var relacion = await _context.DirigentesPoliticos
            .Include(d => d.PartidoPolitico)
            .FirstOrDefaultAsync(d => d.UsuarioId == usuarioId);

        return relacion?.PartidoPolitico;
    }

    public async Task<IndicadoresDirigenteDto> GetIndicadoresAsync(int partidoId)
    {
        return new IndicadoresDirigenteDto
        {
            CandidatosActivos = await _context.Candidatos
                .CountAsync(c => c.PartidoPoliticoId == partidoId && c.Activo),

            CandidatosInactivos = await _context.Candidatos
                .CountAsync(c => c.PartidoPoliticoId == partidoId && !c.Activo),

            AlianzasVigentes = await _context.AlianzasPoliticas
                .CountAsync(a => a.Vigente &&
                    (a.PartidoSolicitanteId == partidoId || a.PartidoAliadoId == partidoId)),

            SolicitudesPendientesDeResponder = await _context.AlianzasPoliticas
                .CountAsync(a => a.PartidoAliadoId == partidoId && a.Estado == EstadoAlianza.Pendiente),

            // Candidatos del partido (propios) que están asignados a un puesto dentro del mismo partido
            CandidatosAsignados = await _context.AsignacionesCandidatoPuesto
                .Include(a => a.Candidato)
                .CountAsync(a => a.PartidoPoliticoId == partidoId && a.Candidato.PartidoPoliticoId == partidoId)
        };
    }
}
