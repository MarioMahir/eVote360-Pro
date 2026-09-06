using eVote360.Core.Constants;
using eVote360.Core.DTOs.Candidatos;
using eVote360.Core.Interfaces.Services;
using eVote360.Filters;
using eVote360.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.DirigentePolitico)]
[DirigenteConPartido]
public class CandidatosController : BaseController
{
    private readonly ICandidatoService _candidatos;
    private readonly IEleccionService _elecciones;

    public CandidatosController(ICandidatoService candidatos, IEleccionService elecciones)
    {
        _candidatos = candidatos;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        return View(await _candidatos.GetAllAsync(PartidoActual.Id));
    }

    public IActionResult Create() => View(new CandidatoCreateDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CandidatoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var resultado = await _candidatos.CreateAsync(dto, PartidoActual.Id);

        if (!resultado.Success)
        {
            AgregarErrores(resultado.Error);
            return View(dto);
        }

        MensajeExito("Candidato creado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var candidato = await _candidatos.GetByIdAsync(id, PartidoActual.Id);

        if (candidato == null)
        {
            MensajeError("No tiene permisos para modificar este candidato.");
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DatosBloqueados = await _elecciones.CandidatoParticipoAsync(id);

        return View(new CandidatoUpdateDto
        {
            Id = candidato.Id,
            Nombre = candidato.Nombre,
            Apellido = candidato.Apellido,
            FotoActual = candidato.FotoUrl,
            Activo = candidato.Activo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CandidatoUpdateDto dto)
    {
        if (id != dto.Id) return NotFound();

        ViewBag.DatosBloqueados = await _elecciones.CandidatoParticipoAsync(id);

        if (!ModelState.IsValid)
            return View(dto);

        var resultado = await _candidatos.UpdateAsync(dto, PartidoActual.Id);

        if (!resultado.Success)
        {
            AgregarErrores(resultado.Error);
            return View(dto);
        }

        MensajeExito("Candidato actualizado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var candidato = await _candidatos.GetByIdAsync(id, PartidoActual.Id);

        if (candidato == null)
        {
            MensajeError("No tiene permisos para activar este candidato.");
            return RedirectToAction(nameof(Index));
        }

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Activar candidato",
            Mensaje = "¿Está seguro que desea activar este candidato?",
            Detalle = candidato.NombreCompleto,
            Accion = nameof(ConfirmarActivar),
            Controlador = "Candidatos",
            Id = id,
            ClaseBoton = "btn-success"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var resultado = await _candidatos.ActivarAsync(id, PartidoActual.Id);

        if (resultado.Success) MensajeExito("Candidato activado.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var candidato = await _candidatos.GetByIdAsync(id, PartidoActual.Id);

        if (candidato == null)
        {
            MensajeError("No tiene permisos para desactivar este candidato.");
            return RedirectToAction(nameof(Index));
        }

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Desactivar candidato",
            Mensaje = "¿Está seguro que desea desactivar este candidato?",
            Detalle = candidato.NombreCompleto,
            Accion = nameof(ConfirmarDesactivar),
            Controlador = "Candidatos",
            Id = id,
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(int id)
    {
        var resultado = await _candidatos.DesactivarAsync(id, PartidoActual.Id);

        if (resultado.Success) MensajeExito("Candidato desactivado.");
        else MensajeError(resultado.Error);

        return RedirectToAction(nameof(Index));
    }
}
