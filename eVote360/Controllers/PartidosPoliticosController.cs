using eVote360.Core.Entities;
using eVote360.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Controllers
{
    // [Authorize(Roles = "Administrador")]
    public class PartidosPoliticosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PartidosPoliticosController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var partidos = await _context.PartidosPoliticos.ToListAsync();

            return View(partidos);
        }

        public IActionResult Create()
        {
            return View(new PartidoPolitico());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PartidoPolitico partido,
            IFormFile logo)
        {
            partido.Siglas = partido.Siglas.Trim().ToUpper();

            if (await _context.PartidosPoliticos
                .AnyAsync(p => p.Siglas == partido.Siglas))
            {
                ModelState.AddModelError(
                    "Siglas",
                    "Ya existe un partido político registrado con estas siglas.");
            }

            if (logo == null)
            {
                ModelState.AddModelError(
                    "LogoUrl",
                    "Debe seleccionar un logo.");
            }


            if (!ModelState.IsValid)
                return View(partido);

            string extension = Path.GetExtension(logo.FileName)
                .ToLower();

            string[] permitidas = { ".jpg", ".jpeg", ".png" };

            if (!permitidas.Contains(extension))
            {
                ModelState.AddModelError(
                    "LogoUrl",
                    "El logo del partido debe ser una imagen válida.");

                return View(partido);
            }

            string nombreArchivo = $"{partido.Siglas}_{Guid.NewGuid()}{extension}";

            string ruta =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "partidos",
                    nombreArchivo);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await logo.CopyToAsync(stream);
            }

            partido.LogoUrl =
                "/uploads/partidos/" + nombreArchivo;

            _context.PartidosPoliticos.Add(partido);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var partido =
                await _context.PartidosPoliticos.FindAsync(id);

            if (partido == null)
                return NotFound();

            return View(partido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PartidoPolitico partido,
            IFormFile? logo)
        {
            if (id != partido.Id)
                return NotFound();

            partido.Siglas = partido.Siglas.Trim().ToUpper();

            bool siglasDuplicadas =
                await _context.PartidosPoliticos
                .AnyAsync(p =>
                    p.Id != partido.Id &&
                    p.Siglas == partido.Siglas);

            if (siglasDuplicadas)
            {
                ModelState.AddModelError(
                    "Siglas",
                    "Ya existe un partido político registrado con estas siglas.");
            }

            if (!ModelState.IsValid)
                return View(partido);

            var partidoDb =
                await _context.PartidosPoliticos
                .FirstAsync(p => p.Id == id);

            partidoDb.Nombre = partido.Nombre;
            partidoDb.Descripcion = partido.Descripcion;
            partidoDb.Siglas = partido.Siglas;
            partidoDb.Activo = partido.Activo;

            if (logo != null)
            {
                string extension =
                    Path.GetExtension(logo.FileName)
                    .ToLower();

                string[] permitidas =
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                if (!permitidas.Contains(extension))
                {
                    ModelState.AddModelError(
                        "LogoUrl",
                        "El logo del partido debe ser una imagen válida.");

                    return View(partido);
                }

                string nombreArchivo =
                    Guid.NewGuid() + extension;

                string ruta =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "partidos",
                        nombreArchivo);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }

                partidoDb.LogoUrl =
                    "/uploads/partidos/" + nombreArchivo;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Activar(int id)
        {
            var partido =
                await _context.PartidosPoliticos.FindAsync(id);

            if (partido == null)
                return NotFound();

            return View(partido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarActivar(int id)
        {
            var partido =
                await _context.PartidosPoliticos.FindAsync(id);

            if (partido == null)
                return NotFound();

            if (partido.Activo)
            {
                TempData["Error"] =
                    "Este partido político ya se encuentra activo.";

                return RedirectToAction(nameof(Index));
            }

            partido.Activo = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Desactivar(int id)
        {
            var partido =
                await _context.PartidosPoliticos.FindAsync(id);

            if (partido == null)
                return NotFound();

            return View(partido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarDesactivar(int id)
        {
            var partido =
                await _context.PartidosPoliticos.FindAsync(id);

            if (partido == null)
                return NotFound();

            if (!partido.Activo)
            {
                TempData["Error"] =
                    "Este partido político ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }

            partido.Activo = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}