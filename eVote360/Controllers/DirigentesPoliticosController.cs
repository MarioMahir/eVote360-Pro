using eVote360.Core.Constants;
using eVote360.Core.DTOs.DirigentesPoliticos;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class DirigentesPoliticosController : BaseController
{
    private readonly IDirigentePoliticoService _service;
    private readonly IEleccionService _elecciones;

    public DirigentesPoliticosController(IDirigentePoliticoService service, IEleccionService elecciones)
    {
        _service = service;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        return View(await _service.GetAllAsync());
    }

    public async Task<IActionResult> Create()
    {
        await CargarCombos();
        return View(new DirigentePoliticoCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DirigentePoliticoCreateDto dto)
    {
        if (dto.UsuarioId <= 0)
            ModelState.AddModelError(nameof(dto.UsuarioId), "El dirigente político es requerido.");

        if (dto.PartidoPoliticoId <= 0)
            ModelState.AddModelError(nameof(dto.PartidoPoliticoId), "El partido político es requerido.");

        if (!ModelState.IsValid)
        {
            await CargarCombos();
            return View(dto);
        }

        var resultado = await _service.CreateAsync(dto);

        if (!resultado.Success)
        {
            AgregarErrores(resultado.Error);
            await CargarCombos();
            return View(dto);
        }

        MensajeExito("Asignación de dirigente político creada correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var relacion = await _service.GetByIdAsync(id);

        if (relacion == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Eliminar asignación",
            Mensaje = "¿Está seguro que desea desvincular este dirigente político de este partido?",
            Detalle = $"{relacion.Usuario.Nombre} {relacion.Usuario.Apellido} — {relacion.PartidoPolitico.Nombre} ({relacion.PartidoPolitico.Siglas})",
            Accion = nameof(ConfirmDelete),
            Controlador = "DirigentesPoliticos",
            Id = id,
            TextoAceptar = "Eliminar relación",
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var resultado = await _service.DeleteAsync(id);

        if (resultado.Success) MensajeExito("La relación fue eliminada.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCombos()
    {
        var dirigentes = await _service.GetDirigentesDisponiblesAsync();
        var partidos = await _service.GetPartidosDisponiblesAsync();

        ViewBag.Dirigentes = dirigentes.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Nombre} {x.Apellido} - {x.NombreUsuario}"
        });

        ViewBag.Partidos = partidos.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Nombre} - {x.Siglas}"
        });
    }
}
