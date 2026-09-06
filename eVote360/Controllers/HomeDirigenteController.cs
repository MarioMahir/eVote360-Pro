using eVote360.Core.Constants;
using eVote360.Core.Interfaces.Services;
using eVote360.Filters;
using eVote360.ViewModels.Dirigente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.DirigentePolitico)]
[DirigenteConPartido]
public class HomeDirigenteController : BaseController
{
    private readonly IDirigenteService _dirigentes;
    private readonly IEleccionService _elecciones;

    public HomeDirigenteController(IDirigenteService dirigentes, IEleccionService elecciones)
    {
        _dirigentes = dirigentes;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        var partido = PartidoActual;

        return View(new HomeDirigenteViewModel
        {
            Partido = partido,
            Indicadores = await _dirigentes.GetIndicadoresAsync(partido.Id),
            ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync()
        });
    }
}
