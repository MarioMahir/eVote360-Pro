using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

// [Authorize(Roles = "Administrador")]
public class CiudadanosController : Controller
{
    private readonly ICiudadanoService _ciudadanoService;

    public CiudadanosController(
        ICiudadanoService ciudadanoService)
    {
        _ciudadanoService = ciudadanoService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = false;

        var ciudadanos =
            await _ciudadanoService.GetAllAsync();

        return View(ciudadanos);
    }

    public IActionResult Create()
    {
        return View(new Ciudadano
        {
            Activo = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Ciudadano ciudadano)
    {
        if (!ModelState.IsValid)
            return View(ciudadano);

        var result =
            await _ciudadanoService.CreateAsync(ciudadano);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error);
            return View(ciudadano);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var ciudadano =
            await _ciudadanoService.GetByIdAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Ciudadano ciudadano)
    {
        if (id != ciudadano.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(ciudadano);

        var result =
            await _ciudadanoService.UpdateAsync(ciudadano);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error);
            return View(ciudadano);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var ciudadano =
            await _ciudadanoService.GetByIdAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivarConfirmado(int id)
    {
        var result =
            await _ciudadanoService.ActivarAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] =
                "Ciudadano activado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var ciudadano =
            await _ciudadanoService.GetByIdAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarConfirmado(int id)
    {
        var result =
            await _ciudadanoService.DesactivarAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] =
                "Ciudadano desactivado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}