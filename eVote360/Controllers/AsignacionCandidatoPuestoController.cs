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
public class AsignacionCandidatoPuestoController : BaseController
{
    private readonly IAsignacionCandidatoPuestoService _asignaciones;
    private readonly IEleccionService _elecciones;

    public AsignacionCandidatoPuestoController(IAsignacionCandidatoPuestoService asignaciones, IEleccionService elecciones)
    {
        _asignaciones = asignaciones;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        return View(await _asignaciones.GetAllAsync(PartidoActual.Id));
    }

    public async Task<IActionResult> Create()
    {
        var model = new AsignacionCreateViewModel();
        await CargarCombos(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AsignacionCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombos(model);
            return View(model);
        }

        var resultado = await _asignaciones.CreateAsync(PartidoActual.Id, model.CandidatoId, model.PuestoElectivoId);

        if (!resultado.Success)
        {
            AgregarErrores(resultado.Error);
            await CargarCombos(model);
            return View(model);
        }

        MensajeExito("La asignación fue creada correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var asignacion = await _asignaciones.GetByIdAsync(id, PartidoActual.Id);

        if (asignacion == null)
        {
            MensajeError("No tiene permisos para eliminar esta asignación.");
            return RedirectToAction(nameof(Index));
        }

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Eliminar relación",
            Mensaje = "¿Está seguro que desea desvincular este candidato de este puesto electivo?",
            Detalle = $"{asignacion.Candidato.NombreCompleto} — {asignacion.PuestoElectivo.Nombre}",
            Accion = nameof(ConfirmDelete),
            Controlador = "AsignacionCandidatoPuesto",
            Id = id,
            TextoAceptar = "Eliminar relación",
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var resultado = await _asignaciones.DeleteAsync(id, PartidoActual.Id);

        if (resultado.Success) MensajeExito("La relación fue eliminada.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCombos(AsignacionCreateViewModel model)
    {
        var candidatos = await _asignaciones.GetCandidatosDisponiblesAsync(PartidoActual.Id);
        var puestos = await _asignaciones.GetPuestosDisponiblesAsync(PartidoActual.Id);

        model.Candidatos = candidatos.Select(c => new SelectListItem(c.Texto, c.Id.ToString())).ToList();
        model.Puestos = puestos.Select(p => new SelectListItem(p.Nombre, p.Id.ToString())).ToList();
    }
}
