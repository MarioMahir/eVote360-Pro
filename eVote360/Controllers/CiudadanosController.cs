using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace eVote360.Controllers;

[Authorize(Roles = "Administrador")]
public class CiudadanosController : Controller
{
    private readonly ICiudadanoService _ciudadanoService;

    public CiudadanosController(ICiudadanoService ciudadanoService)
    {
        _ciudadanoService = ciudadanoService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = false;

        var ciudadanos = await _ciudadanoService.GetAllAsync();

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
    public async Task<IActionResult> Create(Ciudadano ciudadano)
    {
        if (!ModelState.IsValid)
            return View(ciudadano);

        var result = await _ciudadanoService.CreateAsync(ciudadano);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error);
            return View(ciudadano);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var ciudadano = await _ciudadanoService.GetByIdAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Ciudadano ciudadano)
    {
        if (id != ciudadano.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(ciudadano);

        var result = await _ciudadanoService.UpdateAsync(ciudadano);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error);
            return View(ciudadano);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var ciudadano = await _ciudadanoService.GetByIdAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivarConfirmado(int id)
    {
        var result = await _ciudadanoService.ActivarAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Ciudadano activado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var ciudadano = await _ciudadanoService.GetByIdAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarConfirmado(int id)
    {
        var result = await _ciudadanoService.DesactivarAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Ciudadano desactivado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // MÉTODOS PARA EL PORTAL DEL ELECTOR
    // ==========================================

    [AllowAnonymous] // Permite que los electores verifiquen su cédula sin loguearse como Admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarVotacion(string numeroDocumento)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento))
        {
            TempData["Error"] = "Por favor, ingrese un número de documento válido.";
            return RedirectToAction("Index", "Home");
        }

        var ciudadano = await _ciudadanoService.GetByNumeroDocumentoAsync(numeroDocumento.Trim());

        if (ciudadano == null)
        {
            TempData["Error"] = "El número de documento no se encuentra registrado.";
            return RedirectToAction("Index", "Home");
        }

        if (!ciudadano.Activo)
        {
            TempData["Error"] = "Este ciudadano no se encuentra activo para votar.";
            return RedirectToAction("Index", "Home");
        }

        // REDIRECCIÓN CONTROLADA TEMPORALMENTE:
        // Evita buscar la vista que no existe. Retorna al Home con aviso de éxito.
        TempData["Success"] = $"¡Cédula encontrada con éxito! Bienvenido, {ciudadano.Nombre}.";
        return RedirectToAction("Index", "Home");
    }
}