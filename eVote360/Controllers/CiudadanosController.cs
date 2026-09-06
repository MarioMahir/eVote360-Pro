using eVote360.Core.Constants;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels;
using eVote360.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class CiudadanosController : BaseController
{
    private readonly ICiudadanoService _service;
    private readonly IEleccionService _elecciones;

    public CiudadanosController(ICiudadanoService service, IEleccionService elecciones)
    {
        _service = service;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        return View(await _service.GetAllAsync());
    }

    public IActionResult Create() => View(new CiudadanoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CiudadanoViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.CreateAsync(new Ciudadano
        {
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            CorreoElectronico = model.CorreoElectronico,
            NumeroDocumento = model.NumeroDocumento,
            Activo = model.Activo
        });

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Ciudadano creado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var ciudadano = await _service.GetByIdAsync(id);

        if (ciudadano == null) return NotFound();

        return View(new CiudadanoViewModel
        {
            Id = ciudadano.Id,
            Nombre = ciudadano.Nombre,
            Apellido = ciudadano.Apellido,
            CorreoElectronico = ciudadano.CorreoElectronico,
            NumeroDocumento = ciudadano.NumeroDocumento,
            Activo = ciudadano.Activo,
            DocumentoBloqueado = await _elecciones.CiudadanoParticipoAsync(id)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CiudadanoViewModel model)
    {
        if (id != model.Id) return NotFound();

        model.DocumentoBloqueado = await _elecciones.CiudadanoParticipoAsync(id);

        if (!ModelState.IsValid)
            return View(model);

        var result = await _service.UpdateAsync(new Ciudadano
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            CorreoElectronico = model.CorreoElectronico,
            NumeroDocumento = model.NumeroDocumento,
            Activo = model.Activo
        });

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Ciudadano actualizado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var ciudadano = await _service.GetByIdAsync(id);

        if (ciudadano == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Activar ciudadano",
            Mensaje = "¿Está seguro que desea activar este ciudadano?",
            Detalle = $"{ciudadano.Nombre} {ciudadano.Apellido} ({ciudadano.NumeroDocumento})",
            Accion = nameof(ActivarConfirmado),
            Controlador = "Ciudadanos",
            Id = id,
            ClaseBoton = "btn-success"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivarConfirmado(int id)
    {
        var result = await _service.ActivarAsync(id);

        if (result.Success) MensajeExito("Ciudadano activado correctamente.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var ciudadano = await _service.GetByIdAsync(id);

        if (ciudadano == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Desactivar ciudadano",
            Mensaje = "¿Está seguro que desea desactivar este ciudadano?",
            Detalle = $"{ciudadano.Nombre} {ciudadano.Apellido} ({ciudadano.NumeroDocumento})",
            Accion = nameof(DesactivarConfirmado),
            Controlador = "Ciudadanos",
            Id = id,
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarConfirmado(int id)
    {
        var result = await _service.DesactivarAsync(id);

        if (result.Success) MensajeExito("Ciudadano desactivado correctamente.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }
}
