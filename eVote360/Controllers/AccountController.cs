using System.Security.Claims;
using eVote360.Core.Constants;
using eVote360.Core.DTOs.Auth;
using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

public class AccountController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IDirigenteService _dirigentes;

    public AccountController(IAuthService authService, IDirigenteService dirigentes)
    {
        _authService = authService;
        _dirigentes = dirigentes;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _authService.LoginAsync(dto.NombreUsuario, dto.Contrasena);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(dto);
        }

        var usuario = result.Usuario!;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.GivenName, $"{usuario.Nombre} {usuario.Apellido}"),
            new(ClaimTypes.Role, usuario.Rol)
        };

        if (usuario.Rol == Roles.DirigentePolitico)
        {
            var partido = await _dirigentes.GetPartidoDeUsuarioAsync(usuario.Id);

            if (partido != null)
                claims.Add(new Claim("PartidoPoliticoId", partido.Id.ToString()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return usuario.Rol == Roles.Administrador
            ? RedirectToAction("Index", "HomeAdmin")
            : RedirectToAction("Index", "HomeDirigente");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction(nameof(Login));

        return View();
    }
}
