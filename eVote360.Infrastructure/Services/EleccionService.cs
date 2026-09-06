using eVote360.Core.DTOs.Elecciones;
using eVote360.Core.Entities;
using eVote360.Core.Enums;
using eVote360.Core.Interfaces.Repositories;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class EleccionService : IEleccionService
{
    private readonly AppDbContext _context;
    private readonly IGenericRepository<Eleccion> _elecciones;

    public EleccionService(AppDbContext context, IGenericRepository<Eleccion> elecciones)
    {
        _context = context;
        _elecciones = elecciones;
    }

    public async Task<List<EleccionListDto>> GetAllAsync()
    {
        var elecciones = await _elecciones.Query()
            .OrderByDescending(e => e.Estado == EstadoEleccion.Activa)
            .ThenByDescending(e => e.FechaEleccion)
            .ThenByDescending(e => e.Id)
            .ToListAsync();

        var candidaturas = await _context.CandidaturasEleccion
            .GroupBy(c => c.EleccionId)
            .Select(g => new
            {
                EleccionId = g.Key,
                Partidos = g.Select(x => x.PartidoPoliticoId).Distinct().Count(),
                Puestos = g.Select(x => x.PuestoElectivoId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.EleccionId);

        var participaciones = await _context.ParticipacionesEleccion
            .GroupBy(p => p.EleccionId)
            .Select(g => new { EleccionId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.EleccionId, x => x.Total);

        return elecciones.Select(e => new EleccionListDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            FechaEleccion = e.FechaEleccion,
            Estado = e.Estado,
            PartidosParticipantes = candidaturas.TryGetValue(e.Id, out var c) ? c.Partidos : 0,
            PuestosDisputados = candidaturas.TryGetValue(e.Id, out var c2) ? c2.Puestos : 0,
            CiudadanosQueVotaron = participaciones.GetValueOrDefault(e.Id)
        }).ToList();
    }

    public Task<Eleccion?> GetByIdAsync(int id) => _elecciones.GetByIdAsync(id);

    public Task<bool> ExisteEleccionActivaAsync() =>
        _elecciones.AnyAsync(e => e.Estado == EstadoEleccion.Activa);

    public Task<Eleccion?> GetEleccionActivaAsync() =>
        _elecciones.FirstOrDefaultAsync(e => e.Estado == EstadoEleccion.Activa);

    public async Task<List<string>> ValidarConfiguracionAsync()
    {
        var errores = new List<string>();

        var puestosActivos = await _context.PuestosElectivos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        if (puestosActivos.Count == 0)
        {
            errores.Add("No hay puestos electivos activos para realizar una elección.");
            return errores;
        }

        var partidosActivos = await _context.PartidosPoliticos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        if (partidosActivos.Count < 2)
        {
            errores.Add("No hay suficientes partidos políticos para realizar una elección.");
            return errores;
        }

        // Asignaciones vigentes válidas: candidato activo, puesto activo, partido activo
        var asignaciones = await _context.AsignacionesCandidatoPuesto
            .Include(a => a.Candidato)
            .Include(a => a.PuestoElectivo)
            .Where(a => a.Candidato.Activo && a.PuestoElectivo.Activo)
            .ToListAsync();

        foreach (var partido in partidosActivos)
        {
            var puestosCubiertos = asignaciones
                .Where(a => a.PartidoPoliticoId == partido.Id)
                .Select(a => a.PuestoElectivoId)
                .ToHashSet();

            var faltantes = puestosActivos
                .Where(p => !puestosCubiertos.Contains(p.Id))
                .Select(p => p.Nombre)
                .ToList();

            if (faltantes.Count > 0)
            {
                errores.Add(
                    $"El partido político {partido.Nombre} ({partido.Siglas}) no tiene candidatos activos asignados " +
                    $"para los siguientes puestos electivos: {string.Join(", ", faltantes)}.");
            }
        }

        return errores;
    }

    public async Task<(bool Success, string Error)> CreateAsync(string nombre, DateTime fechaEleccion)
    {
        nombre = nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre de la elección es requerido.");

        if (await ExisteEleccionActivaAsync())
            return (false, "No se puede crear una nueva elección mientras exista una elección activa.");

        var errores = await ValidarConfiguracionAsync();

        if (errores.Count > 0)
            return (false, string.Join("\n", errores));

        await _elecciones.AddAsync(new Eleccion
        {
            Nombre = nombre,
            FechaEleccion = fechaEleccion.Date,
            Estado = EstadoEleccion.Pendiente
        });

        await _elecciones.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        var eleccion = await _elecciones.GetByIdAsync(id);

        if (eleccion == null)
            return (false, "La elección seleccionada no existe.");

        if (eleccion.Estado != EstadoEleccion.Pendiente)
            return (false, "Solo se pueden activar elecciones en estado pendiente.");

        if (await ExisteEleccionActivaAsync())
            return (false, "No se puede activar esta elección porque ya existe una elección activa.");

        var errores = await ValidarConfiguracionAsync();

        if (errores.Count > 0)
        {
            errores.Add("No se puede activar esta elección porque la configuración electoral actual no está completa.");
            return (false, string.Join("\n", errores));
        }

        // Congela únicamente las relaciones que participan (no los datos de las entidades)
        var asignaciones = await _context.AsignacionesCandidatoPuesto
            .Include(a => a.Candidato)
            .Include(a => a.PuestoElectivo)
            .Include(a => a.PartidoPolitico)
            .Where(a => a.Candidato.Activo && a.PuestoElectivo.Activo && a.PartidoPolitico.Activo)
            .ToListAsync();

        foreach (var a in asignaciones)
        {
            _context.CandidaturasEleccion.Add(new CandidaturaEleccion
            {
                EleccionId = eleccion.Id,
                PartidoPoliticoId = a.PartidoPoliticoId,
                CandidatoId = a.CandidatoId,
                PuestoElectivoId = a.PuestoElectivoId
            });
        }

        eleccion.Estado = EstadoEleccion.Activa;
        eleccion.FechaActivacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> FinalizarAsync(int id)
    {
        var eleccion = await _elecciones.GetByIdAsync(id);

        if (eleccion == null)
            return (false, "La elección seleccionada no existe.");

        if (eleccion.Estado == EstadoEleccion.Finalizada)
            return (false, "Esta elección ya se encuentra finalizada.");

        if (eleccion.Estado != EstadoEleccion.Activa)
            return (false, "Solo se pueden finalizar elecciones activas.");

        eleccion.Estado = EstadoEleccion.Finalizada;
        eleccion.FechaFinalizacion = DateTime.Now;

        await _elecciones.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<List<ResultadoPuestoDto>?> GetResultadosAsync(int id)
    {
        var eleccion = await _elecciones.GetByIdAsync(id);

        if (eleccion == null || eleccion.Estado != EstadoEleccion.Finalizada)
            return null;

        var candidaturas = await _context.CandidaturasEleccion
            .Include(c => c.Candidato)
            .Include(c => c.PartidoPolitico)
            .Include(c => c.PuestoElectivo)
            .Where(c => c.EleccionId == id)
            .ToListAsync();

        var votos = await _context.Votos
            .Where(v => v.EleccionId == id)
            .GroupBy(v => new { v.PuestoElectivoId, v.CandidatoId, v.PartidoPoliticoId })
            .Select(g => new { g.Key.PuestoElectivoId, g.Key.CandidatoId, g.Key.PartidoPoliticoId, Total = g.Count() })
            .ToListAsync();

        var resultados = new List<ResultadoPuestoDto>();

        foreach (var grupoPuesto in candidaturas.GroupBy(c => c.PuestoElectivoId).OrderBy(g => g.First().PuestoElectivo.Nombre))
        {
            var puesto = grupoPuesto.First().PuestoElectivo;
            var votosPuesto = votos.Where(v => v.PuestoElectivoId == puesto.Id).ToList();
            var totalVotos = votosPuesto.Sum(v => v.Total);

            var opciones = grupoPuesto
                .OrderBy(c => c.PartidoPolitico.Nombre)
                .Select(c => new OpcionResultadoDto
                {
                    CandidatoId = c.CandidatoId,
                    Candidato = c.Candidato.NombreCompleto,
                    FotoUrl = c.Candidato.FotoUrl,
                    Partido = $"{c.PartidoPolitico.Nombre} ({c.PartidoPolitico.Siglas})",
                    LogoUrl = c.PartidoPolitico.LogoUrl,
                    Votos = votosPuesto
                        .Where(v => v.CandidatoId == c.CandidatoId && v.PartidoPoliticoId == c.PartidoPoliticoId)
                        .Sum(v => v.Total)
                })
                .ToList();

            opciones.Add(new OpcionResultadoDto
            {
                CandidatoId = null,
                Candidato = "Ninguno",
                Partido = "No aplica",
                Votos = votosPuesto.Where(v => v.CandidatoId == null).Sum(v => v.Total)
            });

            foreach (var o in opciones)
                o.Porcentaje = totalVotos == 0 ? 0 : Math.Round(o.Votos * 100m / totalVotos, 2);

            opciones = opciones.OrderByDescending(o => o.Votos).ThenBy(o => o.Candidato).ToList();

            var maximo = opciones.Max(o => o.Votos);
            var enPrimerLugar = opciones.Where(o => o.Votos == maximo).ToList();
            var empate = totalVotos > 0 && enPrimerLugar.Count > 1;

            foreach (var o in enPrimerLugar)
            {
                o.Ganador = !empate && totalVotos > 0;
                o.EmpatePrimerLugar = empate;
            }

            resultados.Add(new ResultadoPuestoDto
            {
                PuestoElectivoId = puesto.Id,
                PuestoNombre = puesto.Nombre,
                TotalVotos = totalVotos,
                Empate = empate,
                Opciones = opciones
            });
        }

        return resultados;
    }

    public Task<List<int>> GetAniosConEleccionesAsync() =>
        _elecciones.Query()
            .Select(e => e.FechaEleccion.Year)
            .Distinct()
            .OrderByDescending(a => a)
            .ToListAsync();

    public async Task<List<ResumenEleccionDto>> GetResumenPorAnioAsync(int anio)
    {
        var elecciones = await _elecciones.Query()
            .Where(e => e.FechaEleccion.Year == anio)
            .OrderByDescending(e => e.FechaEleccion)
            .ToListAsync();

        var ids = elecciones.Select(e => e.Id).ToList();

        var candidaturas = await _context.CandidaturasEleccion
            .Where(c => ids.Contains(c.EleccionId))
            .Select(c => new { c.EleccionId, c.PartidoPoliticoId, c.CandidatoId })
            .ToListAsync();

        var participaciones = await _context.ParticipacionesEleccion
            .Where(p => ids.Contains(p.EleccionId))
            .GroupBy(p => p.EleccionId)
            .Select(g => new { EleccionId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.EleccionId, x => x.Total);

        return elecciones.Select(e => new ResumenEleccionDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            FechaEleccion = e.FechaEleccion,
            Estado = e.Estado.ToString(),
            PartidosParticipantes = candidaturas.Where(c => c.EleccionId == e.Id).Select(c => c.PartidoPoliticoId).Distinct().Count(),
            // Candidatos reales: un mismo candidato por varios partidos (alianzas) cuenta una sola vez
            CandidatosParticipantes = candidaturas.Where(c => c.EleccionId == e.Id).Select(c => c.CandidatoId).Distinct().Count(),
            CiudadanosQueVotaron = participaciones.GetValueOrDefault(e.Id)
        }).ToList();
    }

    public Task<bool> PuestoParticipoAsync(int puestoId) =>
        _context.CandidaturasEleccion.AnyAsync(c => c.PuestoElectivoId == puestoId);

    public Task<bool> PartidoParticipoAsync(int partidoId) =>
        _context.CandidaturasEleccion.AnyAsync(c => c.PartidoPoliticoId == partidoId);

    public Task<bool> CandidatoParticipoAsync(int candidatoId) =>
        _context.CandidaturasEleccion.AnyAsync(c => c.CandidatoId == candidatoId);

    public Task<bool> CiudadanoParticipoAsync(int ciudadanoId) =>
        _context.ParticipacionesEleccion.AnyAsync(p => p.CiudadanoId == ciudadanoId);
}
