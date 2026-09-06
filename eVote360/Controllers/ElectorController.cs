using eVote360.Core.Interfaces.Services;
using eVote360.Helpers;
using eVote360.ViewModels.Elector;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

/// <summary>
/// Proceso de votación: validación OCR de la cédula, código por correo,
/// boleta por puesto y finalización. El estado vive en la sesión.
/// </summary>
[AllowAnonymous]
public class ElectorController : BaseController
{
    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png"];

    private readonly IVotacionService _votacion;
    private readonly IOcrService _ocr;
    private readonly ICiudadanoService _ciudadanos;

    public ElectorController(IVotacionService votacion, IOcrService ocr, ICiudadanoService ciudadanos)
    {
        _votacion = votacion;
        _ocr = ocr;
        _ciudadanos = ciudadanos;
    }

    // ---------- Paso 1: validación de identidad mediante OCR ----------

    public IActionResult ValidarIdentidad()
    {
        var sesion = SesionElector.Leer(HttpContext.Session);

        if (sesion == null || !sesion.Iniciado)
            return RedirectToAction("Index", "Home");

        if (sesion.OcrValidado)
            return RedirectToAction(nameof(Codigo));

        return View(new CedulaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidarIdentidad(CedulaViewModel model)
    {
        var sesion = SesionElector.Leer(HttpContext.Session);

        if (sesion == null || !sesion.Iniciado)
            return RedirectToAction("Index", "Home");

        if (model.Imagen == null || model.Imagen.Length == 0)
        {
            ModelState.AddModelError(nameof(model.Imagen), "Debe subir una imagen de su cédula para validar su identidad.");
            return View(model);
        }

        var extension = Path.GetExtension(model.Imagen.FileName).ToLowerInvariant();

        if (!ExtensionesPermitidas.Contains(extension) || !model.Imagen.ContentType.StartsWith("image/"))
        {
            ModelState.AddModelError(nameof(model.Imagen), "El archivo seleccionado no tiene un formato de imagen válido.");
            return View(model);
        }

        await using var stream = model.Imagen.OpenReadStream();
        var ocr = await _ocr.ExtraerTextoAsync(stream);

        if (!ocr.Success)
        {
            ModelState.AddModelError(string.Empty, ocr.Error);
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(ocr.Texto) || !ocr.Texto.Any(char.IsDigit))
        {
            ModelState.AddModelError(string.Empty,
                "No fue posible leer correctamente el número de documento en la imagen cargada. Por favor, suba una imagen más clara.");
            return View(model);
        }

        if (!_ocr.CoincideDocumento(ocr.Texto, sesion.NumeroDocumento))
        {
            MensajeError("Los datos extraídos de la foto no coinciden con los datos previamente ingresados por el elector.");
            return RedirectToAction(nameof(ValidarIdentidad));
        }

        sesion.OcrValidado = true;
        sesion.Guardar(HttpContext.Session);

        var envio = await _votacion.GenerarYEnviarCodigoAsync(sesion.CiudadanoId, sesion.EleccionId);

        if (!envio.Success)
        {
            sesion.OcrValidado = false;
            sesion.Guardar(HttpContext.Session);
            ModelState.AddModelError(string.Empty, envio.Error);
            return View(model);
        }

        MensajeExito("Identidad validada. Le enviamos un código de verificación a su correo electrónico.");
        return RedirectToAction(nameof(Codigo));
    }

    // ---------- Paso 2: código de verificación por correo ----------

    public async Task<IActionResult> Codigo()
    {
        var sesion = SesionElector.Leer(HttpContext.Session);

        if (sesion == null || !sesion.Iniciado || !sesion.OcrValidado)
            return RedirectToAction("Index", "Home");

        if (sesion.CodigoValidado)
            return RedirectToAction(nameof(Puestos));

        return View(new CodigoViewModel { CorreoOculto = await CorreoOcultoAsync(sesion.CiudadanoId) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Codigo(CodigoViewModel model)
    {
        var sesion = SesionElector.Leer(HttpContext.Session);

        if (sesion == null || !sesion.Iniciado || !sesion.OcrValidado)
            return RedirectToAction("Index", "Home");

        model.CorreoOculto = await CorreoOcultoAsync(sesion.CiudadanoId);

        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _votacion.ValidarCodigoAsync(sesion.CiudadanoId, sesion.EleccionId, model.Codigo);

        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.Error);
            return View(model);
        }

        sesion.CodigoValidado = true;
        sesion.Guardar(HttpContext.Session);

        return RedirectToAction(nameof(Puestos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReenviarCodigo()
    {
        var sesion = SesionElector.Leer(HttpContext.Session);

        if (sesion == null || !sesion.Iniciado || !sesion.OcrValidado)
            return RedirectToAction("Index", "Home");

        var envio = await _votacion.GenerarYEnviarCodigoAsync(sesion.CiudadanoId, sesion.EleccionId);

        if (envio.Success)
            MensajeExito("Le enviamos un nuevo código de verificación a su correo electrónico.");
        else
            MensajeError(envio.Error);

        return RedirectToAction(nameof(Codigo));
    }

    // ---------- Paso 3: puestos electivos y selección ----------

    public async Task<IActionResult> Puestos()
    {
        var sesion = ObtenerSesionHabilitada();

        if (sesion == null)
            return RedirectToAction("Index", "Home");

        var puestos = await _votacion.GetPuestosBoletaAsync(sesion.EleccionId, sesion.Selecciones);

        ViewBag.TodosSeleccionados = puestos.All(p => p.Seleccionado);

        return View(puestos);
    }

    public async Task<IActionResult> Votar(int id)
    {
        var sesion = ObtenerSesionHabilitada();

        if (sesion == null)
            return RedirectToAction("Index", "Home");

        if (!await _votacion.PuestoPerteneceAEleccionAsync(sesion.EleccionId, id))
        {
            MensajeError("El puesto seleccionado no pertenece a la elección activa.");
            return RedirectToAction(nameof(Puestos));
        }

        var opciones = await _votacion.GetOpcionesPuestoAsync(sesion.EleccionId, id);
        var puestos = await _votacion.GetPuestosBoletaAsync(sesion.EleccionId, sesion.Selecciones);

        var model = new VotoPuestoViewModel
        {
            PuestoElectivoId = id,
            PuestoNombre = puestos.First(p => p.PuestoElectivoId == id).Nombre,
            Opciones = opciones,
            CandidatoSeleccionado = sesion.Selecciones.TryGetValue(id, out var actual) ? (actual ?? 0) : null
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Votar(VotoPuestoViewModel model)
    {
        var sesion = ObtenerSesionHabilitada();

        if (sesion == null)
            return RedirectToAction("Index", "Home");

        if (!await _votacion.PuestoPerteneceAEleccionAsync(sesion.EleccionId, model.PuestoElectivoId))
        {
            MensajeError("El puesto seleccionado no pertenece a la elección activa.");
            return RedirectToAction(nameof(Puestos));
        }

        var opciones = await _votacion.GetOpcionesPuestoAsync(sesion.EleccionId, model.PuestoElectivoId);

        if (model.CandidatoSeleccionado == null ||
            (model.CandidatoSeleccionado != 0 && opciones.All(o => o.CandidatoId != model.CandidatoSeleccionado)))
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar un candidato antes de votar.");

            var puestos = await _votacion.GetPuestosBoletaAsync(sesion.EleccionId, sesion.Selecciones);
            model.PuestoNombre = puestos.First(p => p.PuestoElectivoId == model.PuestoElectivoId).Nombre;
            model.Opciones = opciones;

            return View(model);
        }

        // 0 representa la opción "Ninguno"
        sesion.Selecciones[model.PuestoElectivoId] = model.CandidatoSeleccionado == 0 ? null : model.CandidatoSeleccionado;
        sesion.Guardar(HttpContext.Session);

        MensajeExito("Selección guardada. Puede modificarla hasta que finalice la votación.");
        return RedirectToAction(nameof(Puestos));
    }

    // ---------- Paso 4: finalización ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar()
    {
        var sesion = ObtenerSesionHabilitada();

        if (sesion == null)
            return RedirectToAction("Index", "Home");

        var resultado = await _votacion.FinalizarAsync(sesion.CiudadanoId, sesion.EleccionId, sesion.Selecciones);

        if (!resultado.Success)
        {
            MensajeError(resultado.Error);
            return RedirectToAction(nameof(Puestos));
        }

        // Cierra el proceso: el resumen se muestra una sola vez y no permite volver a la boleta
        SesionElector.Limpiar(HttpContext.Session);
        TempData["ResumenVotacion"] = System.Text.Json.JsonSerializer.Serialize(resultado.Resumen);

        return RedirectToAction(nameof(Resumen));
    }

    public IActionResult Resumen()
    {
        if (TempData["ResumenVotacion"] is not string json)
            return RedirectToAction("Index", "Home");

        var resumen = System.Text.Json.JsonSerializer.Deserialize<Core.DTOs.Votacion.ResumenVotacionDto>(json);

        return View(resumen);
    }

    public IActionResult Cancelar()
    {
        SesionElector.Limpiar(HttpContext.Session);
        return RedirectToAction("Index", "Home");
    }

    // ---------- Utilidades ----------

    private SesionElector? ObtenerSesionHabilitada()
    {
        var sesion = SesionElector.Leer(HttpContext.Session);

        return sesion is { Iniciado: true, OcrValidado: true, CodigoValidado: true } ? sesion : null;
    }

    private async Task<string> CorreoOcultoAsync(int ciudadanoId)
    {
        var ciudadano = await _ciudadanos.GetByIdAsync(ciudadanoId);
        var correo = ciudadano?.CorreoElectronico ?? string.Empty;
        var arroba = correo.IndexOf('@');

        if (arroba <= 2) return correo;

        return correo[..2] + new string('*', Math.Max(1, arroba - 2)) + correo[arroba..];
    }
}
