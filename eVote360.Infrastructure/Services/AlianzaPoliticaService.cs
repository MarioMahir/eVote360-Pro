using eVote360.Core.DTOs.Alianzas;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class AlianzaPoliticaService : IAlianzaPoliticaService
{
    private readonly AppDbContext _context;
    private readonly IEleccionService _elecciones;

    public AlianzaPoliticaService(AppDbContext context, IEleccionService elecciones)
    {
        _context = context;
        _elecciones = elecciones;
    }

    public async Task<List<SolicitudAlianzaDto>> GetPendientesRecibidasAsync(int partidoId)
    {
        return await _context.AlianzasPoliticas
            .Include(a => a.PartidoSolicitante)
            .Where(a => a.PartidoAliadoId == partidoId && a.Estado == EstadoAlianza.Pendiente)
            .OrderByDescending(a => a.FechaSolicitud)
            .Select(a => new SolicitudAlianzaDto
            {
                Id = a.Id,
                PartidoId = a.PartidoSolicitanteId,
                PartidoNombre = a.PartidoSolicitante.Nombre,
                PartidoSiglas = a.PartidoSolicitante.Siglas,
                LogoUrl = a.PartidoSolicitante.LogoUrl,
                FechaSolicitud = a.FechaSolicitud,
                Estado = a.Estado
            })
            .ToListAsync();
    }

    public async Task<List<SolicitudAlianzaDto>> GetRealizadasAsync(int partidoId)
    {
        return await _context.AlianzasPoliticas
            .Include(a => a.PartidoAliado)
            .Where(a => a.PartidoSolicitanteId == partidoId)
            .OrderByDescending(a => a.FechaSolicitud)
            .Select(a => new SolicitudAlianzaDto
            {
                Id = a.Id,
                PartidoId = a.PartidoAliadoId,
                PartidoNombre = a.PartidoAliado.Nombre,
                PartidoSiglas = a.PartidoAliado.Siglas,
                LogoUrl = a.PartidoAliado.LogoUrl,
                FechaSolicitud = a.FechaSolicitud,
                Estado = a.Estado
            })
            .ToListAsync();
    }

    public async Task<List<AlianzaVigenteDto>> GetVigentesAsync(int partidoId)
    {
        var alianzas = await _context.AlianzasPoliticas
            .Include(a => a.PartidoSolicitante)
            .Include(a => a.PartidoAliado)
            .Where(a => a.Vigente && (a.PartidoSolicitanteId == partidoId || a.PartidoAliadoId == partidoId))
            .OrderByDescending(a => a.FechaRespuesta)
            .ToListAsync();

        return alianzas.Select(a =>
        {
            var otro = a.PartidoSolicitanteId == partidoId ? a.PartidoAliado : a.PartidoSolicitante;

            return new AlianzaVigenteDto
            {
                Id = a.Id,
                PartidoAliadoId = otro.Id,
                PartidoAliadoNombre = otro.Nombre,
                PartidoAliadoSiglas = otro.Siglas,
                LogoUrl = otro.LogoUrl,
                FechaAceptacion = a.FechaRespuesta ?? a.FechaSolicitud
            };
        }).ToList();
    }

    public async Task<List<PartidoPolitico>> GetPartidosDisponiblesAsync(int partidoId)
    {
        // Partidos con alianza vigente o solicitud pendiente en cualquier dirección quedan fuera
        var excluidos = await _context.AlianzasPoliticas
            .Where(a => (a.PartidoSolicitanteId == partidoId || a.PartidoAliadoId == partidoId) &&
                        (a.Vigente || a.Estado == EstadoAlianza.Pendiente))
            .Select(a => a.PartidoSolicitanteId == partidoId ? a.PartidoAliadoId : a.PartidoSolicitanteId)
            .ToListAsync();

        return await _context.PartidosPoliticos
            .Where(p => p.Activo && p.Id != partidoId && !excluidos.Contains(p.Id))
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public Task<AlianzaPolitica?> GetByIdAsync(int id) =>
        _context.AlianzasPoliticas
            .Include(a => a.PartidoSolicitante)
            .Include(a => a.PartidoAliado)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<bool> ExisteAlianzaVigenteAsync(int partidoA, int partidoB) =>
        _context.AlianzasPoliticas.AnyAsync(a => a.Vigente &&
            ((a.PartidoSolicitanteId == partidoA && a.PartidoAliadoId == partidoB) ||
             (a.PartidoSolicitanteId == partidoB && a.PartidoAliadoId == partidoA)));

    public async Task<(bool Success, string Error)> CrearSolicitudAsync(int partidoId, int partidoAliadoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede crear una solicitud de alianza mientras exista una elección activa.");

        if (partidoId == partidoAliadoId)
            return (false, "No puede crear una solicitud de alianza hacia su propio partido político.");

        var propio = await _context.PartidosPoliticos.FindAsync(partidoId);

        if (propio == null || !propio.Activo)
            return (false, "El partido político asignado a este usuario se encuentra inactivo.");

        var aliado = await _context.PartidosPoliticos.FindAsync(partidoAliadoId);

        if (aliado == null)
            return (false, "El partido político seleccionado no existe.");

        if (!aliado.Activo)
            return (false, "No puede crear una solicitud de alianza con un partido político inactivo.");

        if (await ExisteAlianzaVigenteAsync(partidoId, partidoAliadoId))
            return (false, "Ya existe una alianza vigente con este partido político.");

        if (await _context.AlianzasPoliticas.AnyAsync(a =>
                a.Estado == EstadoAlianza.Pendiente &&
                a.PartidoSolicitanteId == partidoId && a.PartidoAliadoId == partidoAliadoId))
            return (false, "Ya existe una solicitud de alianza pendiente enviada a este partido político.");

        if (await _context.AlianzasPoliticas.AnyAsync(a =>
                a.Estado == EstadoAlianza.Pendiente &&
                a.PartidoSolicitanteId == partidoAliadoId && a.PartidoAliadoId == partidoId))
            return (false, "Ya existe una solicitud de alianza pendiente enviada por este partido político.");

        _context.AlianzasPoliticas.Add(new AlianzaPolitica
        {
            PartidoSolicitanteId = partidoId,
            PartidoAliadoId = partidoAliadoId,
            FechaSolicitud = DateTime.Now,
            Estado = EstadoAlianza.Pendiente,
            Vigente = false
        });

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> AceptarAsync(int id, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede aceptar una solicitud de alianza mientras exista una elección activa.");

        var solicitud = await GetByIdAsync(id);

        if (solicitud == null)
            return (false, "La solicitud de alianza seleccionada no existe o ya fue eliminada.");

        if (solicitud.PartidoAliadoId != partidoId)
            return (false, "No tiene permisos para responder esta solicitud de alianza.");

        if (solicitud.Estado != EstadoAlianza.Pendiente)
            return (false, "Esta solicitud de alianza ya fue respondida.");

        if (!solicitud.PartidoSolicitante.Activo || !solicitud.PartidoAliado.Activo)
            return (false, "Ambos partidos políticos deben estar activos para formalizar la alianza.");

        if (await ExisteAlianzaVigenteAsync(solicitud.PartidoSolicitanteId, solicitud.PartidoAliadoId))
            return (false, "Ya existe una alianza vigente con este partido político.");

        solicitud.Estado = EstadoAlianza.Aceptada;
        solicitud.Vigente = true;
        solicitud.FechaRespuesta = DateTime.Now;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> RechazarAsync(int id, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede rechazar una solicitud de alianza mientras exista una elección activa.");

        var solicitud = await _context.AlianzasPoliticas.FindAsync(id);

        if (solicitud == null)
            return (false, "La solicitud de alianza seleccionada no existe o ya fue eliminada.");

        if (solicitud.PartidoAliadoId != partidoId)
            return (false, "No tiene permisos para responder esta solicitud de alianza.");

        if (solicitud.Estado != EstadoAlianza.Pendiente)
            return (false, "Esta solicitud de alianza ya fue respondida.");

        solicitud.Estado = EstadoAlianza.Rechazada;
        solicitud.FechaRespuesta = DateTime.Now;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> EliminarSolicitudAsync(int id, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede eliminar una solicitud de alianza mientras exista una elección activa.");

        var solicitud = await _context.AlianzasPoliticas.FindAsync(id);

        if (solicitud == null)
            return (false, "La solicitud de alianza seleccionada no existe o ya fue eliminada.");

        if (solicitud.PartidoSolicitanteId != partidoId)
            return (false, "No tiene permisos para eliminar esta solicitud de alianza.");

        if (solicitud.Estado == EstadoAlianza.Aceptada)
            return (false, "No se puede eliminar una solicitud aceptada porque ya generó una alianza vigente. " +
                           "Para terminarla debe eliminar la alianza desde el listado de alianzas vigentes.");

        _context.AlianzasPoliticas.Remove(solicitud);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> EliminarAlianzaAsync(int id, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede eliminar una alianza política mientras exista una elección activa.");

        var alianza = await _context.AlianzasPoliticas.FindAsync(id);

        if (alianza == null || !alianza.Vigente)
            return (false, "La alianza política seleccionada no existe o ya fue eliminada.");

        if (alianza.PartidoSolicitanteId != partidoId && alianza.PartidoAliadoId != partidoId)
            return (false, "No tiene permisos para eliminar esta alianza política.");

        var otroPartido = alianza.PartidoSolicitanteId == partidoId
            ? alianza.PartidoAliadoId
            : alianza.PartidoSolicitanteId;

        // Candidatos aliados asignados en cualquiera de las dos direcciones
        var hayAliadosAsignados = await _context.AsignacionesCandidatoPuesto
            .Include(a => a.Candidato)
            .AnyAsync(a =>
                (a.PartidoPoliticoId == partidoId && a.Candidato.PartidoPoliticoId == otroPartido) ||
                (a.PartidoPoliticoId == otroPartido && a.Candidato.PartidoPoliticoId == partidoId));

        if (hayAliadosAsignados)
        {
            return (false, "No se puede eliminar esta alianza porque existen candidatos aliados asignados entre estos partidos. " +
                           "Primero deben eliminarse las asignaciones correspondientes desde el módulo Asignar candidato a puesto.");
        }

        // Eliminación lógica: la solicitud aceptada se conserva como histórico
        alianza.Vigente = false;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}
