using eVote360.Core.DTOs.Usuarios;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace eVote360.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios =
            await _usuarioService.GetAllAsync();

        return View(usuarios);
    }

    public IActionResult Create()
    {
        return View(new UsuarioCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        UsuarioCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        UsuarioCreateDto dto = new()
        {
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            CorreoElectronico = model.CorreoElectronico,
            NombreUsuario = model.NombreUsuario,
            Contrasena = model.Contrasena,
            ConfirmarContrasena = model.ConfirmarContrasena,
            Rol = model.Rol,
            Activo = model.Activo
        };

        var result =
            await _usuarioService.CreateAsync(dto);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario =
            await _usuarioService.GetByIdAsync(id);

        if (usuario == null)
            return NotFound();

        UsuarioEditViewModel model = new()
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            CorreoElectronico = usuario.CorreoElectronico,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol,
            Activo = usuario.Activo
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        UsuarioEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        UsuarioUpdateDto dto = new()
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            CorreoElectronico = model.CorreoElectronico,
            NombreUsuario = model.NombreUsuario,
            Contrasena = model.Contrasena,
            ConfirmarContrasena = model.ConfirmarContrasena,
            Rol = model.Rol,
            Activo = model.Activo
        };

        var result =
            await _usuarioService.UpdateAsync(dto);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var usuario =
            await _usuarioService.GetByIdAsync(id);

        if (usuario == null)
            return NotFound();

        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var result =
            await _usuarioService.ActivarAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var usuario =
            await _usuarioService.GetByIdAsync(id);

        if (usuario == null)
            return NotFound();

        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(int id)
    {
        var result =
            await _usuarioService.DesactivarAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }
}
