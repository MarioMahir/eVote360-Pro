using eVote360.Core.Constants;
using eVote360.Core.Interfaces.Services;
using eVote360.Filters;
using eVote360.ViewModels;
using eVote360.ViewModels.Dirigente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.DirigentePolitico)]
[DirigenteConPartido]
public class AlianzasPoliticasController : BaseController
{
    private readonly IAlianzaPoliticaService _alianzas;
    private readonly IEleccionService _elecciones;

    public AlianzasPoliticasController(IAlianzaPoliticaService alianzas, IEleccionService elecciones)
    {
        _alianzas = alianzas;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        var partidoId = PartidoActual.Id;

        return View(new AlianzasIndexViewModel
        {
            PendientesDeResponder = await _alianzas.GetPendientesRecibidasAsync(partidoId),
            Realizadas = await _alianzas.GetRealizadasAsync(partidoId),
            Vigentes = await _alianzas.GetVigentesAsync(partidoId),
            ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync()
        });
    }

    public async Task<IActionResult> Create()
    {
        var model = new AlianzaCreateViewModel();
        await CargarPartidos(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AlianzaCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarPartidos(model);
            return View(model);
        }

        var resultado = await _alianzas.CrearSolicitudAsync(PartidoActual.Id, model.PartidoAliadoId);

        if (!resultado.Success)
        {
            AgregarErrores(resultado.Error);
            await CargarPartidos(model);
            return View(model);
        }

        MensajeExito("Solicitud de alianza enviada. Queda en espera de respuesta.");
        return RedirectToAction(nameof(Index));
    }

    public Task<IActionResult> Aceptar(int id) => Confirmar(id,
        "Aceptar solicitud de alianza", "¿Está seguro que desea aceptar la alianza con el partido {0}?",
        nameof(ConfirmarAceptar), "Aceptar", "btn-success", esRecibida: true);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarAceptar(int id)
    {
        var resultado = await _alianzas.AceptarAsync(id, PartidoActual.Id);

        if (resultado.Success) MensajeExito("Alianza aceptada. Ahora es una alianza vigente.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    public Task<IActionResult> Rechazar(int id) => Confirmar(id,
        "Rechazar solicitud de alianza", "¿Está seguro que desea rechazar la alianza con el partido {0}?",
        nameof(ConfirmarRechazar), "Rechazar", "btn-danger", esRecibida: true);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarRechazar(int id)
    {
        var resultado = await _alianzas.RechazarAsync(id, PartidoActual.Id);

        if (resultado.Success) MensajeExito("Solicitud de alianza rechazada.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    public Task<IActionResult> EliminarSolicitud(int id) => Confirmar(id,
        "Eliminar solicitud de alianza", "¿Está seguro que desea eliminar la solicitud de alianza con el partido {0}?",
        nameof(ConfirmarEliminarSolicitud), "Eliminar", "btn-danger", esRecibida: false);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarEliminarSolicitud(int id)
    {
        var resultado = await _alianzas.EliminarSolicitudAsync(id, PartidoActual.Id);

        if (resultado.Success) MensajeExito("Solicitud de alianza eliminada.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    public Task<IActionResult> EliminarAlianza(int id) => Confirmar(id,
        "Eliminar alianza vigente", "¿Está seguro que desea eliminar la alianza política con el partido {0}?",
        nameof(ConfirmarEliminarAlianza), "Eliminar alianza", "btn-danger", esRecibida: null);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarEliminarAlianza(int id)
    {
        var resultado = await _alianzas.EliminarAlianzaAsync(id, PartidoActual.Id);

        if (resultado.Success) MensajeExito("La alianza política fue eliminada.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    /// <param name="esRecibida">true: el otro partido es el solicitante; false: es el aliado; null: el que no sea el propio.</param>
    private async Task<IActionResult> Confirmar(int id, string titulo, string plantilla, string accion,
        string textoAceptar, string claseBoton, bool? esRecibida)
    {
        var alianza = await _alianzas.GetByIdAsync(id);

        if (alianza == null)
        {
            MensajeError("La solicitud de alianza seleccionada no existe o ya fue eliminada.");
            return RedirectToAction(nameof(Index));
        }

        var propio = PartidoActual.Id;

        var otro = esRecibida switch
        {
            true => alianza.PartidoSolicitante,
            false => alianza.PartidoAliado,
            null => alianza.PartidoSolicitanteId == propio ? alianza.PartidoAliado : alianza.PartidoSolicitante
        };

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = titulo,
            Mensaje = string.Format(plantilla, $"{otro.Nombre} ({otro.Siglas})"),
            Accion = accion,
            Controlador = "AlianzasPoliticas",
            Id = id,
            TextoAceptar = textoAceptar,
            ClaseBoton = claseBoton
        });
    }

    private async Task CargarPartidos(AlianzaCreateViewModel model)
    {
        var partidos = await _alianzas.GetPartidosDisponiblesAsync(PartidoActual.Id);

        model.Partidos = partidos.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.Nombre} ({p.Siglas})"
        }).ToList();
    }
}
