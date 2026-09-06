using eVote360.Core.Constants;
using eVote360.Core.DTOs.Usuarios;
using eVote360.Core.Interfaces.Services;
using eVote360.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class UsuariosController : BaseController
{
    private readonly IUsuarioService _usuarioService;
    private readonly IEleccionService _elecciones;

    public UsuariosController(IUsuarioService usuarioService, IEleccionService elecciones)
    {
        _usuarioService = usuarioService;
        _elecciones = elecciones;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ExisteEleccionActiva = await _elecciones.ExisteEleccionActivaAsync();
        ViewBag.UsuarioActualId = UsuarioActualId;
        return View(await _usuarioService.GetAllAsync());
    }

    public IActionResult Create() => View(new UsuarioCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _usuarioService.CreateAsync(new UsuarioCreateDto
        {
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            CorreoElectronico = model.CorreoElectronico,
            NombreUsuario = model.NombreUsuario,
            Contrasena = model.Contrasena,
            ConfirmarContrasena = model.ConfirmarContrasena,
            Rol = model.Rol,
            Activo = model.Activo
        });

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Usuario creado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);

        if (usuario == null) return NotFound();

        return View(new UsuarioEditViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            CorreoElectronico = usuario.CorreoElectronico,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol,
            Activo = usuario.Activo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UsuarioEditViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var result = await _usuarioService.UpdateAsync(new UsuarioUpdateDto
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
        }, UsuarioActualId);

        if (!result.Success)
        {
            AgregarErrores(result.Error);
            return View(model);
        }

        MensajeExito("Usuario actualizado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Activar(int id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);

        if (usuario == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Activar usuario",
            Mensaje = "¿Está seguro que desea activar este usuario?",
            Detalle = $"{usuario.Nombre} {usuario.Apellido} ({usuario.NombreUsuario})",
            Accion = nameof(ConfirmarActivar),
            Controlador = "Usuarios",
            Id = id,
            ClaseBoton = "btn-success"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarActivar(int id)
    {
        var result = await _usuarioService.ActivarAsync(id);

        if (result.Success) MensajeExito("Usuario activado.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Desactivar(int id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);

        if (usuario == null) return NotFound();

        return View("Confirmar", new ConfirmacionViewModel
        {
            Titulo = "Desactivar usuario",
            Mensaje = "¿Está seguro que desea desactivar este usuario?",
            Detalle = $"{usuario.Nombre} {usuario.Apellido} ({usuario.NombreUsuario})",
            Accion = nameof(ConfirmarDesactivar),
            Controlador = "Usuarios",
            Id = id,
            ClaseBoton = "btn-danger"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDesactivar(int id)
    {
        var result = await _usuarioService.DesactivarAsync(id, UsuarioActualId);

        if (result.Success) MensajeExito("Usuario desactivado.");
        else MensajeError(result.Error);

        return RedirectToAction(nameof(Index));
    }
}
