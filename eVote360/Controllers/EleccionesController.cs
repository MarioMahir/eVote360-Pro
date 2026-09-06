using eVote360.Core.Constants;
using eVote360.Core.Enums;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels;
using eVote360.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class EleccionesController : BaseController
{
    private readonly IEleccionService _elecciones;

    public EleccionesController(IEleccionService elecciones)
    {
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        return View(await _elecciones.GetAllAsync());
    }

    public async Task<IActionResult> Create()
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
        {
            MensajeError("No se puede crear una nueva elección mientras exista una elección activa.");
            return RedirectToAction(nameof(Index));
        }

        return View(new EleccionCreateViewModel { FechaEleccion = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EleccionCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _elecciones.CreateAsync(model.Nombre, model.FechaEleccion!.Value);

        if (!resultado.Success)
        {
            AgregarErrores(resultado.Error);
            return View(model);
        }

        MensajeExito("Elección creada en estado pendiente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var eleccion = await _elecciones.GetByIdAsync(id);

        if (eleccion == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Activar elección",
            Mensaje = "¿Está seguro que desea activar esta elección?",
            Detalle = $"{eleccion.Nombre} ({eleccion.FechaEleccion:dd/MM/yyyy}). A partir de ese momento los ciudadanos habilitados podrán votar y se bloquearán los mantenimientos.",
            Accion = nameof(ConfirmarActivar),
            Controlador = "Elecciones",
            Id = id,
            TextoAceptar = "Activar",
            ClaseBoton = "btn-success"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var resultado = await _elecciones.ActivarAsync(id);

        if (resultado.Success)
            MensajeExito("La elección fue activada. El proceso de votación está abierto.");
        else
            MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Finalizar(int id)
    {
        var eleccion = await _elecciones.GetByIdAsync(id);

        if (eleccion == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Finalizar elección",
            Mensaje = "¿Está seguro que desea finalizar esta elección?",
            Detalle = $"{eleccion.Nombre}. No se permitirán nuevos votos y se habilitará la consulta de resultados.",
            Accion = nameof(ConfirmarFinalizar),
            Controlador = "Elecciones",
            Id = id,
            TextoAceptar = "Finalizar",
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarFinalizar(int id)
    {
        var resultado = await _elecciones.FinalizarAsync(id);

        if (resultado.Success)
            MensajeExito("La elección fue finalizada. Ya puede consultar los resultados.");
        else
            MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Resultados(int id)
    {
        var eleccion = await _elecciones.GetByIdAsync(id);

        if (eleccion == null) return NotFound();

        if (eleccion.Estado != EstadoEleccion.Finalizada)
        {
            MensajeError("Los resultados solo están disponibles para elecciones finalizadas.");
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Eleccion = eleccion;

        return View(await _elecciones.GetResultadosAsync(id));
    }
}
