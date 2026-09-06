using eVote360.Core.Constants;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels;
using eVote360.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class PartidosPoliticosController : BaseController
{
    private readonly IPartidoPoliticoService _service;
    private readonly IEleccionService _elecciones;

    public PartidosPoliticosController(IPartidoPoliticoService service, IEleccionService elecciones)
    {
        _service = service;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        return View(await _service.GetAllAsync());
    }

    public IActionResult Create() => View(new PartidoPoliticoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartidoPoliticoViewModel model)
    {
        if (model.Logo == null || model.Logo.Length == 0)
            ModelState.AddModelError(nameof(model.Logo), "El logo del partido es requerido.");

        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.CreateAsync(new PartidoPolitico
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Siglas = model.Siglas,
            Activo = model.Activo
        }, model.Logo!);

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Partido político creado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var partido = await _service.GetByIdAsync(id);

        if (partido == null) return NotFound();

        return View(new PartidoPoliticoViewModel
        {
            Id = partido.Id,
            Nombre = partido.Nombre,
            Descripcion = partido.Descripcion,
            Siglas = partido.Siglas,
            LogoActual = partido.LogoUrl,
            Activo = partido.Activo,
            DatosBloqueados = await _elecciones.PartidoParticipoAsync(id)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PartidoPoliticoViewModel model)
    {
        if (id != model.Id) return NotFound();

        model.DatosBloqueados = await _elecciones.PartidoParticipoAsync(id);

        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.UpdateAsync(new PartidoPolitico
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Siglas = model.Siglas,
            Activo = model.Activo
        }, model.Logo);

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Partido político actualizado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var partido = await _service.GetByIdAsync(id);

        if (partido == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Activar partido político",
            Mensaje = "¿Está seguro que desea activar este partido político?",
            Detalle = $"{partido.Nombre} ({partido.Siglas})",
            Accion = nameof(ConfirmarActivar),
            Controlador = "PartidosPoliticos",
            Id = id,
            ClaseBoton = "btn-success"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var result = await _service.ActivarAsync(id);

        if (result.Success) MensajeExito("Partido político activado.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var partido = await _service.GetByIdAsync(id);

        if (partido == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Desactivar partido político",
            Mensaje = "¿Está seguro que desea desactivar este partido político?",
            Detalle = $"{partido.Nombre} ({partido.Siglas})",
            Accion = nameof(ConfirmarDesactivar),
            Controlador = "PartidosPoliticos",
            Id = id,
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(int id)
    {
        var result = await _service.DesactivarAsync(id);

        if (result.Success) MensajeExito("Partido político desactivado.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }
}
