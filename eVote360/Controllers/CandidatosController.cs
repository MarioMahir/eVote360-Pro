using eVote360.Core.DTOs.Candidatos;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

public class CandidatosController : Controller
{
    private readonly ICandidatoService _candidatoService;

    private const int PartidoTemporalId = 1;

    public CandidatosController(
        ICandidatoService candidatoService)
    {
        _candidatoService = candidatoService;
    }

    public async Task<IActionResult> Index()
    {
        var candidatos =
            await _candidatoService.GetAllAsync(
                PartidoTemporalId);

        return View(candidatos);
    }

    public IActionResult Create()
    {
        return View(new CandidatoCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CandidatoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var resultado =
            await _candidatoService.CreateAsync(
                dto,
                PartidoTemporalId);

        if (!resultado.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.Error);

            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var candidato =
            await _candidatoService.GetByIdAsync(
                id,
                PartidoTemporalId);

        if (candidato == null)
            return NotFound();

        var dto = new CandidatoUpdateDto
        {
            Id = candidato.Id,
            Nombre = candidato.Nombre,
            Apellido = candidato.Apellido,
            Activo = candidato.Activo
        };

        ViewBag.FotoActual =
            candidato.FotoUrl;

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        CandidatoUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var resultado =
            await _candidatoService.UpdateAsync(
                dto,
                PartidoTemporalId);

        if (!resultado.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.Error);

            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var candidato =
            await _candidatoService.GetByIdAsync(
                id,
                PartidoTemporalId);

        if (candidato == null)
            return NotFound();

        return View(candidato);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var resultado =
            await _candidatoService.ActivarAsync(
                id,
                PartidoTemporalId);

        if (!resultado.Success)
        {
            TempData["Error"] =
                resultado.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var candidato =
            await _candidatoService.GetByIdAsync(
                id,
                PartidoTemporalId);

        if (candidato == null)
            return NotFound();

        return View(candidato);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(
        int id)
    {
        var resultado =
            await _candidatoService.DesactivarAsync(
                id,
                PartidoTemporalId);

        if (!resultado.Success)
        {
            TempData["Error"] =
                resultado.Error;
        }

        return RedirectToAction(nameof(Index));
    }
}