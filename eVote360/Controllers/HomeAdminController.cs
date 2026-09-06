using eVote360.Core.Constants;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class HomeAdminController : BaseController
{
    private readonly IEleccionService _elecciones;

    public HomeAdminController(IEleccionService elecciones)
    {
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index(int? anio, bool consultar = false)
    {
        var anios = await _elecciones.GetAniosConEleccionesAsync();

        var model = new ResumenAnioViewModel
        {
            Anio = anio ?? anios.FirstOrDefault(),
            Anios = anios.Select(a => new SelectListItem(a.ToString(), a.ToString())).ToList()
        };

        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();

        if (!consultar)
            return View(model);

        if (anio == null)
        {
            ModelState.AddModelError(nameof(model.Anio), "Debe seleccionar un año para consultar el resumen electoral.");
            return View(model);
        }

        if (!anios.Contains(anio.Value))
        {
            ModelState.AddModelError(nameof(model.Anio), "El año seleccionado no tiene elecciones registradas.");
            return View(model);
        }

        model.Resultados = await _elecciones.GetResumenPorAnioAsync(anio.Value);

        return View(model);
    }
}
