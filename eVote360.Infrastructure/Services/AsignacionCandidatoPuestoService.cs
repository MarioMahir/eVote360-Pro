using eVote360.Core.DTOs.Asignaciones;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class AsignacionCandidatoPuestoService : IAsignacionCandidatoPuestoService
{
    private readonly AppDbContext _context;
    private readonly IEleccionService _elecciones;
    private readonly IAlianzaPoliticaService _alianzas;

    public AsignacionCandidatoPuestoService(
        AppDbContext context,
        IEleccionService elecciones,
        IAlianzaPoliticaService alianzas)
    {
        _context = context;
        _elecciones = elecciones;
        _alianzas = alianzas;
    }

    public async Task<List<AsignacionListDto>> GetAllAsync(int partidoId)
    {
        return await _context.AsignacionesCandidatoPuesto
            .Include(a => a.Candidato).ThenInclude(c => c.PartidoPolitico)
            .Include(a => a.PuestoElectivo)
            .Where(a => a.PartidoPoliticoId == partidoId)
            .OrderBy(a => a.PuestoElectivo.Nombre)
            .Select(a => new AsignacionListDto
            {
                Id = a.Id,
                CandidatoNombre = a.Candidato.Nombre,
                CandidatoApellido = a.Candidato.Apellido,
                FotoUrl = a.Candidato.FotoUrl,
                PartidoOrigen = a.Candidato.PartidoPolitico.Nombre,
                PartidoOrigenSiglas = a.Candidato.PartidoPolitico.Siglas,
                PuestoElectivo = a.PuestoElectivo.Nombre,
                EsAliado = a.Candidato.PartidoPoliticoId != partidoId
            })
            .ToListAsync();
    }

    public Task<AsignacionCandidatoPuesto?> GetByIdAsync(int id, int partidoId) =>
        _context.AsignacionesCandidatoPuesto
            .Include(a => a.Candidato)
            .Include(a => a.PuestoElectivo)
            .FirstOrDefaultAsync(a => a.Id == id && a.PartidoPoliticoId == partidoId);

    public async Task<List<CandidatoDisponibleDto>> GetCandidatosDisponiblesAsync(int partidoId)
    {
        var yaAsignados = await _context.AsignacionesCandidatoPuesto
            .Where(a => a.PartidoPoliticoId == partidoId)
            .Select(a => a.CandidatoId)
            .ToListAsync();

        var propios = await _context.Candidatos
            .Where(c => c.PartidoPoliticoId == partidoId && c.Activo && !yaAsignados.Contains(c.Id))
            .OrderBy(c => c.Nombre).ThenBy(c => c.Apellido)
            .Select(c => new CandidatoDisponibleDto
            {
                Id = c.Id,
                Texto = c.Nombre + " " + c.Apellido + " (candidato propio)",
                EsAliado = false
            })
            .ToListAsync();

        var partidosAliados = await _context.AlianzasPoliticas
            .Where(a => a.Vigente && (a.PartidoSolicitanteId == partidoId || a.PartidoAliadoId == partidoId))
            .Select(a => a.PartidoSolicitanteId == partidoId ? a.PartidoAliadoId : a.PartidoSolicitanteId)
            .ToListAsync();

        // Candidato aliado: activo, de partido aliado activo, con puesto asignado en su partido de origen
        var aliados = await _context.AsignacionesCandidatoPuesto
            .Include(a => a.Candidato).ThenInclude(c => c.PartidoPolitico)
            .Include(a => a.PuestoElectivo)
            .Where(a => partidosAliados.Contains(a.PartidoPoliticoId) &&
                        a.Candidato.PartidoPoliticoId == a.PartidoPoliticoId &&
                        a.Candidato.Activo &&
                        a.Candidato.PartidoPolitico.Activo &&
                        !yaAsignados.Contains(a.CandidatoId))
            .OrderBy(a => a.Candidato.Nombre)
            .Select(a => new CandidatoDisponibleDto
            {
                Id = a.CandidatoId,
                Texto = a.Candidato.Nombre + " " + a.Candidato.Apellido +
                        " (aliado: " + a.Candidato.PartidoPolitico.Siglas + ", " + a.PuestoElectivo.Nombre + ")",
                EsAliado = true
            })
            .ToListAsync();

        return propios.Concat(aliados).ToList();
    }

    public async Task<List<PuestoElectivo>> GetPuestosDisponiblesAsync(int partidoId)
    {
        var ocupados = await _context.AsignacionesCandidatoPuesto
            .Where(a => a.PartidoPoliticoId == partidoId)
            .Select(a => a.PuestoElectivoId)
            .ToListAsync();

        return await _context.PuestosElectivos
            .Where(p => p.Activo && !ocupados.Contains(p.Id))
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<(bool Success, string Error)> CreateAsync(int partidoId, int candidatoId, int puestoElectivoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede asignar candidatos a puestos mientras exista una elección activa.");

        var partido = await _context.PartidosPoliticos.FindAsync(partidoId);

        if (partido == null || !partido.Activo)
            return (false, "El partido político asignado a este usuario se encuentra inactivo.");

        var candidato = await _context.Candidatos
            .Include(c => c.PartidoPolitico)
            .FirstOrDefaultAsync(c => c.Id == candidatoId);

        if (candidato == null)
            return (false, "El candidato seleccionado no existe.");

        if (!candidato.Activo)
            return (false, "El candidato seleccionado se encuentra inactivo.");

        var puesto = await _context.PuestosElectivos.FindAsync(puestoElectivoId);

        if (puesto == null)
            return (false, "El puesto electivo seleccionado no existe.");

        if (!puesto.Activo)
            return (false, "El puesto electivo seleccionado se encuentra inactivo.");

        if (await _context.AsignacionesCandidatoPuesto.AnyAsync(a =>
                a.PartidoPoliticoId == partidoId && a.PuestoElectivoId == puestoElectivoId))
            return (false, "Este puesto electivo ya tiene un candidato asignado dentro del partido.");

        if (await _context.AsignacionesCandidatoPuesto.AnyAsync(a =>
                a.PartidoPoliticoId == partidoId && a.CandidatoId == candidatoId))
            return (false, "Este candidato ya está asignado a un puesto dentro del partido.");

        var esAliado = candidato.PartidoPoliticoId != partidoId;

        if (esAliado)
        {
            if (!candidato.PartidoPolitico.Activo)
                return (false, "El partido de origen de este candidato se encuentra inactivo.");

            if (!await _alianzas.ExisteAlianzaVigenteAsync(partidoId, candidato.PartidoPoliticoId))
                return (false, "No existe una alianza vigente con el partido de este candidato.");

            var asignacionOrigen = await _context.AsignacionesCandidatoPuesto
                .FirstOrDefaultAsync(a =>
                    a.PartidoPoliticoId == candidato.PartidoPoliticoId && a.CandidatoId == candidatoId);

            if (asignacionOrigen == null)
                return (false, "Este candidato aliado no tiene un puesto asignado en su partido de origen.");

            if (asignacionOrigen.PuestoElectivoId != puestoElectivoId)
                return (false, "Este candidato en su partido de origen aspira a un puesto diferente al seleccionado.");
        }

        _context.AsignacionesCandidatoPuesto.Add(new AsignacionCandidatoPuesto
        {
            PartidoPoliticoId = partidoId,
            CandidatoId = candidatoId,
            PuestoElectivoId = puestoElectivoId,
            FechaAsignacion = DateTime.Now
        });

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteAsync(int id, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede eliminar una asignación mientras exista una elección activa.");

        var asignacion = await _context.AsignacionesCandidatoPuesto.FindAsync(id);

        if (asignacion == null)
            return (false, "La asignación seleccionada no existe o ya fue eliminada.");

        if (asignacion.PartidoPoliticoId != partidoId)
            return (false, "No tiene permisos para eliminar esta asignación.");

        _context.AsignacionesCandidatoPuesto.Remove(asignacion);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}
