using System.Net;
using System.Security.Cryptography;
using eVote360.Core.DTOs.Votacion;
using eVote360.Core.Entities;
using eVote360.Core.Enums;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using eVote360.Shared.Email;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class VotacionService : IVotacionService
{
    private static readonly TimeSpan VigenciaCodigo = TimeSpan.FromMinutes(5);

    private readonly AppDbContext _context;
    private readonly IEmailService _email;

    public VotacionService(AppDbContext context, IEmailService email)
    {
        _context = context;
        _email = email;
    }

    public async Task<(bool Success, string Error, int CiudadanoId, int EleccionId)> IniciarAsync(string numeroDocumento)
    {
        numeroDocumento = (numeroDocumento ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(numeroDocumento))
            return (false, "El número de documento de identidad es requerido.", 0, 0);

        var ciudadano = await _context.Ciudadanos
            .FirstOrDefaultAsync(c => c.NumeroDocumento == numeroDocumento);

        if (ciudadano == null)
            return (false, "No existe un ciudadano registrado con este número de documento.", 0, 0);

        var eleccion = await _context.Elecciones
            .FirstOrDefaultAsync(e => e.Estado == EstadoEleccion.Activa);

        if (eleccion == null)
            return (false, "No hay ningún proceso electoral en estos momentos.", 0, 0);

        if (!ciudadano.Activo)
            return (false, "Este ciudadano se encuentra inactivo y no puede participar en el proceso de votación.", 0, 0);

        var yaVoto = await _context.ParticipacionesEleccion
            .AnyAsync(p => p.CiudadanoId == ciudadano.Id && p.EleccionId == eleccion.Id);

        if (yaVoto)
            return (false, "Ya ha ejercido su derecho al voto.", 0, 0);

        return (true, string.Empty, ciudadano.Id, eleccion.Id);
    }

    public async Task<(bool Success, string Error)> GenerarYEnviarCodigoAsync(int ciudadanoId, int eleccionId)
    {
        var ciudadano = await _context.Ciudadanos.FindAsync(ciudadanoId);

        if (ciudadano == null)
            return (false, "No existe un ciudadano registrado con este número de documento.");

        if (string.IsNullOrWhiteSpace(ciudadano.CorreoElectronico))
            return (false, "Este ciudadano no tiene un correo electrónico registrado. No es posible continuar con la verificación de identidad.");

        var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var ahora = DateTime.Now;

        _context.CodigosVerificacion.Add(new CodigoVerificacion
        {
            CiudadanoId = ciudadanoId,
            EleccionId = eleccionId,
            Codigo = codigo,
            FechaGeneracion = ahora,
            FechaExpiracion = ahora.Add(VigenciaCodigo),
            Usado = false
        });

        await _context.SaveChangesAsync();

        var cuerpo = $"""
            <p>Hola <strong>{WebUtility.HtmlEncode(ciudadano.Nombre)} {WebUtility.HtmlEncode(ciudadano.Apellido)}</strong>,</p>
            <p>Su código de verificación para continuar con el proceso de votación es:</p>
            <p style="font-size:32px;letter-spacing:6px;font-weight:bold">{codigo}</p>
            <p>Este código tendrá una vigencia de 5 minutos.</p>
            <p>Si usted no inició este proceso, ignore este mensaje.</p>
            """;

        var envio = await _email.EnviarAsync(
            ciudadano.CorreoElectronico,
            $"{ciudadano.Nombre} {ciudadano.Apellido}",
            "Código de verificación para votar",
            cuerpo);

        if (!envio.Success)
            return (false, "No fue posible enviar el código de verificación. Intente nuevamente más tarde.");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ValidarCodigoAsync(int ciudadanoId, int eleccionId, string codigo)
    {
        codigo = (codigo ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(codigo))
            return (false, "Debe ingresar el código de verificación enviado a su correo electrónico.");

        // Solo cuenta el último código generado para este ciudadano y elección
        var ultimo = await _context.CodigosVerificacion
            .Where(c => c.CiudadanoId == ciudadanoId && c.EleccionId == eleccionId)
            .OrderByDescending(c => c.FechaGeneracion)
            .FirstOrDefaultAsync();

        if (ultimo == null || ultimo.Codigo != codigo)
            return (false, "El código de verificación ingresado no es válido.");

        if (ultimo.Usado)
            return (false, "Este código de verificación ya fue utilizado.");

        if (ultimo.FechaExpiracion < DateTime.Now)
            return (false, "El código de verificación ha expirado. Solicite un nuevo código para continuar.");

        ultimo.Usado = true;
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<List<PuestoBoletaDto>> GetPuestosBoletaAsync(int eleccionId, IReadOnlyDictionary<int, int?> selecciones)
    {
        var candidaturas = await _context.CandidaturasEleccion
            .Include(c => c.PuestoElectivo)
            .Include(c => c.Candidato)
            .Where(c => c.EleccionId == eleccionId)
            .ToListAsync();

        return candidaturas
            .GroupBy(c => c.PuestoElectivoId)
            .OrderBy(g => g.First().PuestoElectivo.Nombre)
            .Select(g =>
            {
                var puesto = g.First().PuestoElectivo;
                var seleccionado = selecciones.TryGetValue(puesto.Id, out var candidatoId);

                string? texto = null;

                if (seleccionado)
                {
                    texto = candidatoId == null
                        ? "Ninguno"
                        : g.FirstOrDefault(c => c.CandidatoId == candidatoId)?.Candidato.NombreCompleto ?? "Ninguno";
                }

                return new PuestoBoletaDto
                {
                    PuestoElectivoId = puesto.Id,
                    Nombre = puesto.Nombre,
                    Descripcion = puesto.Descripcion,
                    PartidosParticipantes = g.Select(c => c.PartidoPoliticoId).Distinct().Count(),
                    CandidatosReales = g.Select(c => c.CandidatoId).Distinct().Count(),
                    Seleccionado = seleccionado,
                    SeleccionTexto = texto
                };
            })
            .ToList();
    }

    public async Task<List<OpcionBoletaDto>> GetOpcionesPuestoAsync(int eleccionId, int puestoElectivoId)
    {
        var candidaturas = await _context.CandidaturasEleccion
            .Include(c => c.Candidato)
            .Include(c => c.PartidoPolitico)
            .Where(c => c.EleccionId == eleccionId && c.PuestoElectivoId == puestoElectivoId)
            .ToListAsync();

        // Orden neutral (alfabético por partido) para no favorecer a ningún partido
        return candidaturas
            .OrderBy(c => c.PartidoPolitico.Nombre)
            .Select(c => new OpcionBoletaDto
            {
                CandidatoId = c.CandidatoId,
                Nombre = c.Candidato.NombreCompleto,
                FotoUrl = c.Candidato.FotoUrl,
                PartidoPoliticoId = c.PartidoPoliticoId,
                PartidoNombre = c.PartidoPolitico.Nombre,
                PartidoSiglas = c.PartidoPolitico.Siglas,
                LogoUrl = c.PartidoPolitico.LogoUrl
            })
            .ToList();
    }

    public Task<bool> PuestoPerteneceAEleccionAsync(int eleccionId, int puestoElectivoId) =>
        _context.CandidaturasEleccion.AnyAsync(c => c.EleccionId == eleccionId && c.PuestoElectivoId == puestoElectivoId);

    public async Task<(bool Success, string Error, ResumenVotacionDto? Resumen)> FinalizarAsync(
        int ciudadanoId,
        int eleccionId,
        IReadOnlyDictionary<int, int?> selecciones)
    {
        var eleccion = await _context.Elecciones.FindAsync(eleccionId);

        if (eleccion == null || eleccion.Estado != EstadoEleccion.Activa)
            return (false, "No hay ningún proceso electoral en estos momentos.", null);

        var ciudadano = await _context.Ciudadanos.FindAsync(ciudadanoId);

        if (ciudadano == null || !ciudadano.Activo)
            return (false, "Este ciudadano se encuentra inactivo y no puede participar en el proceso de votación.", null);

        if (await _context.ParticipacionesEleccion.AnyAsync(p => p.CiudadanoId == ciudadanoId && p.EleccionId == eleccionId))
            return (false, "Ya ha ejercido su derecho al voto.", null);

        var candidaturas = await _context.CandidaturasEleccion
            .Include(c => c.Candidato)
            .Include(c => c.PartidoPolitico)
            .Include(c => c.PuestoElectivo)
            .Where(c => c.EleccionId == eleccionId)
            .ToListAsync();

        var puestos = candidaturas
            .GroupBy(c => c.PuestoElectivoId)
            .Select(g => g.First().PuestoElectivo)
            .OrderBy(p => p.Nombre)
            .ToList();

        var faltantes = puestos.Where(p => !selecciones.ContainsKey(p.Id)).Select(p => p.Nombre).ToList();

        if (faltantes.Count > 0)
        {
            return (false,
                $"Debe completar su selección para los siguientes puestos electivos: {string.Join(", ", faltantes)}.",
                null);
        }

        // Toda selección debe corresponder a un puesto de la elección activa
        if (selecciones.Keys.Any(k => puestos.All(p => p.Id != k)))
            return (false, "Una de las selecciones no corresponde a la elección activa.", null);

        var resumen = new ResumenVotacionDto
        {
            CiudadanoNombre = $"{ciudadano.Nombre} {ciudadano.Apellido}",
            CorreoElectronico = ciudadano.CorreoElectronico,
            EleccionNombre = eleccion.Nombre,
            FechaEleccion = eleccion.FechaEleccion
        };

        foreach (var puesto in puestos)
        {
            var candidatoId = selecciones[puesto.Id];
            CandidaturaEleccion? candidatura = null;

            if (candidatoId != null)
            {
                candidatura = candidaturas.FirstOrDefault(c =>
                    c.PuestoElectivoId == puesto.Id && c.CandidatoId == candidatoId);

                if (candidatura == null)
                    return (false, $"El candidato seleccionado para {puesto.Nombre} no participa en este puesto.", null);
            }

            _context.Votos.Add(new Voto
            {
                EleccionId = eleccionId,
                PuestoElectivoId = puesto.Id,
                CandidatoId = candidatura?.CandidatoId,
                PartidoPoliticoId = candidatura?.PartidoPoliticoId,
                Fecha = DateTime.Now
            });

            resumen.Selecciones.Add(new ResumenVotacionItemDto
            {
                Puesto = puesto.Nombre,
                Seleccion = candidatura?.Candidato.NombreCompleto ?? "Ninguno",
                Partido = candidatura == null ? null : $"{candidatura.PartidoPolitico.Nombre} ({candidatura.PartidoPolitico.Siglas})"
            });
        }

        _context.ParticipacionesEleccion.Add(new ParticipacionEleccion
        {
            EleccionId = eleccionId,
            CiudadanoId = ciudadanoId,
            Fecha = DateTime.Now
        });

        await _context.SaveChangesAsync();

        await EnviarResumenAsync(resumen);

        return (true, string.Empty, resumen);
    }

    private async Task EnviarResumenAsync(ResumenVotacionDto resumen)
    {
        var filas = string.Join("", resumen.Selecciones.Select(s =>
            $"<p><strong>Puesto</strong>: {WebUtility.HtmlEncode(s.Puesto)}<br>" +
            $"<strong>Selección</strong>: {WebUtility.HtmlEncode(s.Seleccion)}" +
            (s.Partido == null ? "" : $"<br><strong>Partido</strong>: {WebUtility.HtmlEncode(s.Partido)}") +
            "</p>"));

        var cuerpo = $"""
            <p>Hola <strong>{WebUtility.HtmlEncode(resumen.CiudadanoNombre)}</strong>,</p>
            <p>Su proceso de votación ha sido completado correctamente.</p>
            <p>Resumen de selección:</p>
            <p>Elección: {WebUtility.HtmlEncode(resumen.EleccionNombre)}<br>
               Fecha de la elección: {resumen.FechaEleccion:dd/MM/yyyy}</p>
            {filas}
            <p>Gracias por ejercer su derecho al voto.</p>
            """;

        // El voto ya quedó registrado: un fallo del correo no debe revertirlo.
        await _email.EnviarAsync(
            resumen.CorreoElectronico,
            resumen.CiudadanoNombre,
            "Resumen de su participación electoral",
            cuerpo);
    }
}
