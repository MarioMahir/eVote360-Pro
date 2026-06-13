using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace eVote360.Controllers;

[Authorize(Roles = "Administrador")]
public class PuestosElectivosController : Controller
{
    private readonly IPuestoElectivoService _service;

    public PuestosElectivosController(
        IPuestoElectivoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var puestos =
            await _service.GetAllAsync();

        return View(puestos);
    }

    public IActionResult Create()
    {
        return View(new PuestoElectivo
        {
            Activo = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PuestoElectivo puesto)
    {
        if (!ModelState.IsValid)
            return View(puesto);

        var result =
            await _service.CreateAsync(puesto);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error);

            return View(puesto);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var puesto =
            await _service.GetByIdAsync(id);

        if (puesto == null)
            return NotFound();

        return View(puesto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PuestoElectivo puesto)
    {
        if (id != puesto.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(puesto);

        var result =
            await _service.UpdateAsync(puesto);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error);

            return View(puesto);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var puesto =
            await _service.GetByIdAsync(id);

        if (puesto == null)
            return NotFound();

        return View(puesto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        await _service.ActivarAsync(id);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var puesto =
            await _service.GetByIdAsync(id);

        if (puesto == null)
            return NotFound();

        return View(puesto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(int id)
    {
        await _service.DesactivarAsync(id);

        return RedirectToAction(nameof(Index));
    }
}