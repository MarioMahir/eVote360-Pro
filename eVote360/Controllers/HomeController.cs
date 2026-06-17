using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

public class HomeController : Controller
{
    private readonly ICiudadanoService _ciudadanoService;

    public HomeController(ICiudadanoService ciudadanoService)
    {
        _ciudadanoService = ciudadanoService;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ValidarDocumento(string numeroDocumento)
    {
        var ciudadano =
            await _ciudadanoService
                .GetByNumeroDocumentoAsync(numeroDocumento);

        if (ciudadano == null)
        {
            TempData["Error"] =
                "No existe un ciudadano con ese número de documento.";

            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] =
            $"Ciudadano encontrado: {ciudadano.Nombre} {ciudadano.Apellido}";

        return RedirectToAction(nameof(Index));
    }
}