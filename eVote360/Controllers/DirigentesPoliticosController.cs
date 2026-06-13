using eVote360.Core.DTOs.DirigentesPoliticos;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers;

// [Authorize(Roles = "Administrador")]
public class DirigentesPoliticosController : Controller
{
    private readonly IDirigentePoliticoService _service;

    public DirigentesPoliticosController(
        IDirigentePoliticoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var relaciones =
            await _service.GetAllAsync();

        return View(relaciones);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCombos();

        return View(
            new DirigentePoliticoCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DirigentePoliticoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombos();

            return View(dto);
        }

        var resultado =
            await _service.CreateAsync(dto);

        if (!resultado.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.Error);

            await CargarCombos();

            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var relacion =
            await _service.GetByIdAsync(id);

        if (relacion == null)
            return NotFound();

        return View(relacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var resultado =
            await _service.DeleteAsync(id);

        if (!resultado.Success)
        {
            TempData["Error"] =
                resultado.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCombos()
    {
        var dirigentes =
            await _service.GetDirigentesDisponiblesAsync();

        var partidos =
            await _service.GetPartidosDisponiblesAsync();

        ViewBag.Dirigentes =
            dirigentes.Select(x =>
                new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text =
                        $"{x.Nombre} {x.Apellido} - {x.NombreUsuario}"
                });

        ViewBag.Partidos =
            partidos.Select(x =>
                new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text =
                        $"{x.Nombre} - {x.Siglas}"
                });
    }
}