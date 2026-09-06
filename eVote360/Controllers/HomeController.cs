using eVote360.Core.Constants;
using eVote360.Core.Interfaces.Services;
using eVote360.Helpers;
using eVote360.ViewModels.Elector;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

/// <summary>Pantalla inicial del elector. Los usuarios autenticados van a su Home.</summary>
public class HomeController : BaseController
{
    private readonly IVotacionService _votacion;

    public HomeController(IVotacionService votacion)
    {
        _votacion = votacion;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        var redireccion = RedirigirSegunRol();
        if (redireccion != null) return redireccion;

        return View(new DocumentoViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DocumentoViewModel model)
    {
        var redireccion = RedirigirSegunRol();
        if (redireccion != null) return redireccion;

        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _votacion.IniciarAsync(model.NumeroDocumento);

        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.Error);
            return View(model);
        }

        SesionElector.Limpiar(HttpContext.Session);

        new SesionElector
        {
            CiudadanoId = resultado.CiudadanoId,
            EleccionId = resultado.EleccionId,
            NumeroDocumento = model.NumeroDocumento.Trim()
        }.Guardar(HttpContext.Session);

        return RedirectToAction("ValidarIdentidad", "Elector");
    }

    [AllowAnonymous]
    public IActionResult Error() => View();

    private IActionResult? RedirigirSegunRol()
    {
        if (User.IsInRole(Roles.Administrador))
            return RedirectToAction("Index", "HomeAdmin");

        if (User.IsInRole(Roles.DirigentePolitico))
            return RedirectToAction("Index", "HomeDirigente");

        return null;
    }
}
