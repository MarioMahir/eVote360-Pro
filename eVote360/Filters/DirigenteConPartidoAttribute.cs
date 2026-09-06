using System.Security.Claims;
using eVote360.Core.Constants;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eVote360.Filters;

/// <summary>
/// Garantiza que el usuario autenticado sea un dirigente político con un partido
/// asignado y activo. Deja el partido disponible en HttpContext.Items["Partido"].
/// Si no cumple, cierra la sesión y lo devuelve al inicio de sesión con el mensaje del enunciado.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DirigenteConPartidoAttribute : Attribute, IAsyncActionFilter
{
    public const string ItemKey = "Partido";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        if (!http.User.IsInRole(Roles.DirigentePolitico))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            return;
        }

        var usuarioId = int.Parse(http.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dirigentes = http.RequestServices.GetRequiredService<IDirigenteService>();
        var partido = await dirigentes.GetPartidoDeUsuarioAsync(usuarioId);

        string? error = null;

        if (partido == null)
            error = "No tiene un partido político asignado, por lo tanto no puede iniciar sesión. Por favor, póngase en contacto con un administrador.";
        else if (!partido.Activo)
            error = "El partido político asignado a este usuario se encuentra inactivo.";

        if (error != null)
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (context.Controller is Controller controller)
                controller.TempData["Error"] = error;

            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        http.Items[ItemKey] = partido;

        if (context.Controller is Controller c)
            c.ViewBag.Partido = partido;

        await next();
    }
}
