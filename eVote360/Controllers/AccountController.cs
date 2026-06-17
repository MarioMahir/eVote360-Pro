using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eVote360.Infrastructure.Data; // Tu AppDbContext

namespace eVote360.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string usuario, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            ModelState.AddModelError("", "Por favor, llene todos los campos.");
            return View();
        }

        // Buscamos usando los nombres exactos de tu entidad 'Usuario'
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == usuario.Trim() && u.PasswordHash == contrasena.Trim());

        if (user == null)
        {
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View();
        }

        if (!user.Activo)
        {
            ModelState.AddModelError("", "Este usuario se encuentra inactivo.");
            return View();
        }

        // Creamos la sesión con los roles correspondientes ("Administrador" o "Dirigente")
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.NombreUsuario),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        // Redirección inteligente según el rol que tiene en tu base de datos
        if (user.Rol == "Administrador")
        {
            return RedirectToAction("Index", "Ciudadanos");
        }
        else if (user.Rol == "Dirigente")
        {
            // Cambia "Index" y "Home" por la pantalla que tengas asignada para los dirigentes
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}