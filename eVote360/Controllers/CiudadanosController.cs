using eVote360.Core.Entities;
using eVote360.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Web.Controllers;

// [Authorize(Roles = "Administrador")] NO TOCAR NI BORRAR, SE DEJA COMENTADO PARA FUTURA IMPLEMENTACIÓN DE ROLES Y PERMISOS
public class CiudadanosController : Controller
{
    private readonly AppDbContext _context;

    public CiudadanosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = false;
        return View(await _context.Ciudadanos.ToListAsync());
    }

    // GET CREATE
    public IActionResult Create()
    {
        return View(new Ciudadano
        {
            Activo = true
        });
    }

    // POST CREATE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ciudadano ciudadano)
    {
        ciudadano.NumeroDocumento = ciudadano.NumeroDocumento.Trim();

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.CorreoElectronico == ciudadano.CorreoElectronico))
        {
            ModelState.AddModelError(
                "CorreoElectronico",
                "Ya existe un ciudadano registrado con este correo electrónico.");
        }

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.NumeroDocumento == ciudadano.NumeroDocumento))
        {
            ModelState.AddModelError(
                "NumeroDocumentoIdentidad",
                "Ya existe un ciudadano registrado con este número de documento de identidad.");
        }

        if (!ModelState.IsValid)
            return View(ciudadano);

        _context.Ciudadanos.Add(ciudadano);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET EDIT
    public async Task<IActionResult> Edit(int id)
    {
        var ciudadano = await _context.Ciudadanos.FindAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    // POST EDIT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Ciudadano ciudadano)
    {
        if (id != ciudadano.Id)
            return NotFound();

        ciudadano.NumeroDocumento =
            ciudadano.NumeroDocumento.Trim();

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.CorreoElectronico == ciudadano.CorreoElectronico &&
            x.Id != ciudadano.Id))
        {
            ModelState.AddModelError(
                "CorreoElectronico",
                "Ya existe un ciudadano registrado con este correo electrónico.");
        }

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.NumeroDocumento == ciudadano.NumeroDocumento &&
            x.Id != ciudadano.Id))
        {
            ModelState.AddModelError(
                "NumeroDocumentoIdentidad",
                "Ya existe un ciudadano registrado con este número de documento de identidad.");
        }

        if (!ModelState.IsValid)
            return View(ciudadano);

        _context.Update(ciudadano);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var ciudadano = await _context.Ciudadanos.FindAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivarConfirmado(int id)
    {
        var ciudadano = await _context.Ciudadanos.FindAsync(id);

        if (ciudadano == null)
            return NotFound();

        if (ExisteEleccionActiva())
        {
            TempData["Error"] =
                "No se puede activar un ciudadano mientras exista una elección activa.";

            return RedirectToAction(nameof(Index));
        }

        if (ciudadano.Activo)
        {
            TempData["Error"] =
                "Este ciudadano ya se encuentra activo.";

            return RedirectToAction(nameof(Index));
        }

        ciudadano.Activo = true;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Ciudadano activado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var ciudadano = await _context.Ciudadanos.FindAsync(id);

        if (ciudadano == null)
            return NotFound();

        return View(ciudadano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarConfirmado(int id)
    {
        var ciudadano = await _context.Ciudadanos.FindAsync(id);

        if (ciudadano == null)
            return NotFound();

        if (ExisteEleccionActiva())
        {
            TempData["Error"] =
                "No se puede desactivar un ciudadano mientras exista una elección activa.";

            return RedirectToAction(nameof(Index));
        }

        if (!ciudadano.Activo)
        {
            TempData["Error"] =
                "Este ciudadano ya se encuentra inactivo.";

            return RedirectToAction(nameof(Index));
        }

        ciudadano.Activo = false;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Ciudadano desactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private bool ExisteEleccionActiva()
    {
        // Se implementará cuando exista el módulo Elecciones
        return false;
    }
}