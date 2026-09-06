using System.Security.Claims;
using eVote360.Core.Entities;
using eVote360.Filters;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

/// <summary>Utilidades comunes: mensajes TempData, usuario actual y partido del dirigente.</summary>
public abstract class BaseController : Controller
{
    protected int UsuarioActualId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    /// <summary>Partido del dirigente autenticado, cargado por <see cref="DirigenteConPartidoAttribute"/>.</summary>
    protected PartidoPolitico PartidoActual =>
        (PartidoPolitico)HttpContext.Items[DirigenteConPartidoAttribute.ItemKey]!;

    protected void MensajeExito(string texto) => TempData["Success"] = texto;

    protected void MensajeError(string texto) => TempData["Error"] = texto;

    /// <summary>Agrega al ModelState un error que puede traer varias líneas.</summary>
    protected void AgregarErrores(string error)
    {
        foreach (var linea in error.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            ModelState.AddModelError(string.Empty, linea);
    }
}
