using System.Security.Claims;
using eVote360.Core.DTOs.Auth;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    public AccountController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result =
            await _authService.LoginAsync(
                dto.NombreUsuario,
                dto.Contrasena);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error);

            return View(dto);
        }

        var usuario = result.Usuario!;

        var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            usuario.Id.ToString()),

        new Claim(
            ClaimTypes.Name,
            usuario.NombreUsuario),

        new Claim(
            ClaimTypes.Role,
            usuario.Rol)
    };

        if (usuario.Rol == "Dirigente político")
        {
            var asignacion =
                _context.DirigentesPoliticos
                .FirstOrDefault(x =>
                    x.UsuarioId == usuario.Id);

            if (asignacion != null)
            {
                claims.Add(
                    new Claim(
                        "PartidoPoliticoId",
                        asignacion.PartidoPoliticoId
                            .ToString()));
            }
        }

        var identity =
            new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

        var principal =
            new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (usuario.Rol == "Administrador")
        {
            return RedirectToAction(
                "Index",
                "HomeAdmin");
        }

        return RedirectToAction(
            "Index",
            "HomeDirigente");
    }
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}