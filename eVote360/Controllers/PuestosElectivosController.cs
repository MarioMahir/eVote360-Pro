using eVote360.Core.Constants;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels;
using eVote360.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class PuestosElectivosController : BaseController
{
    private readonly IPuestoElectivoService _service;
    private readonly IEleccionService _elecciones;

    public PuestosElectivosController(IPuestoElectivoService service, IEleccionService elecciones)
    {
        _service = service;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        return View(await _service.GetAllAsync());
    }

    public IActionResult Create() => View(new PuestoElectivoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PuestoElectivoViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.CreateAsync(new PuestoElectivo
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Activo = model.Activo
        });

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Puesto electivo creado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var puesto = await _service.GetByIdAsync(id);

        if (puesto == null) return NotFound();

        return View(new PuestoElectivoViewModel
        {
            Id = puesto.Id,
            Nombre = puesto.Nombre,
            Descripcion = puesto.Descripcion,
            Activo = puesto.Activo,
            NombreBloqueado = await _elecciones.PuestoParticipoAsync(id)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PuestoElectivoViewModel model)
    {
        if (id != model.Id) return NotFound();

        model.NombreBloqueado = await _elecciones.PuestoParticipoAsync(id);

        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.UpdateAsync(new PuestoElectivo
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Activo = model.Activo
        });

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Puesto electivo actualizado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var puesto = await _service.GetByIdAsync(id);

        if (puesto == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Activar puesto electivo",
            Mensaje = "¿Está seguro que desea activar este puesto electivo?",
            Detalle = puesto.Nombre,
            Accion = nameof(ConfirmarActivar),
            Controlador = "PuestosElectivos",
            Id = id,
            ClaseBoton = "btn-success"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var result = await _service.ActivarAsync(id);

        if (result.Success) MensajeExito("Puesto electivo activado.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var puesto = await _service.GetByIdAsync(id);

        if (puesto == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Desactivar puesto electivo",
            Mensaje = "¿Está seguro que desea desactivar este puesto electivo?",
            Detalle = puesto.Nombre,
            Accion = nameof(ConfirmarDesactivar),
            Controlador = "PuestosElectivos",
            Id = id,
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(int id)
    {
        var result = await _service.DesactivarAsync(id);

        if (result.Success) MensajeExito("Puesto electivo desactivado.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }
}
