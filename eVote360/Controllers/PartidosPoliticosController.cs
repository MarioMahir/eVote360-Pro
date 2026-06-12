using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

// [Authorize(Roles = "Administrador")]
public class PartidosPoliticosController : Controller
{
    private readonly IPartidoPoliticoService _service;

    public PartidosPoliticosController(
        IPartidoPoliticoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        return View(
            await _service.GetAllAsync());
    }

    public IActionResult Create()
    {
        return View(new PartidoPolitico());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PartidoPolitico partido,
        IFormFile logo)
    {
        if (!ModelState.IsValid)
            return View(partido);

        var result =
            await _service.CreateAsync(
                partido,
                logo);

        if (!result.Success)
        {
            ModelState.AddModelError(
                "",
                result.Error);

            return View(partido);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var partido =
            await _service.GetByIdAsync(id);

        if (partido == null)
            return NotFound();

        return View(partido);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PartidoPolitico partido,
        IFormFile? logo)
    {
        if (id != partido.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(partido);

        var result =
            await _service.UpdateAsync(
                partido,
                logo);

        if (!result.Success)
        {
            ModelState.AddModelError(
                "",
                result.Error);

            return View(partido);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var partido =
            await _service.GetByIdAsync(id);

        if (partido == null)
            return NotFound();

        return View(partido);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var result =
            await _service.ActivarAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var partido =
            await _service.GetByIdAsync(id);

        if (partido == null)
            return NotFound();

        return View(partido);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(int id)
    {
        var result =
            await _service.DesactivarAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}