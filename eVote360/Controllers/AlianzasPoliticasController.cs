using eVote360.Core.Entities;
using eVote360.Infrastructure.Data;
using eVote360.ViewModels.AlianzaPolitica;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Controllers
{
    public class AlianzasPoliticasController : Controller
    {
        private readonly AppDbContext _context;

        public AlianzasPoliticasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AlianzasPoliticas
        public async Task<IActionResult> Index()
        {
            var alianzas = await _context.AlianzasPoliticas
                .Include(x => x.PartidoSolicitante)
                .Include(x => x.PartidoAliado)
                .OrderByDescending(x => x.FechaSolicitud)
                .ToListAsync();

            return View(alianzas);
        }

        // GET: AlianzasPoliticas/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Partidos = await _context.PartidosPoliticos
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            return View();
        }

        // POST: AlianzasPoliticas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAlianzaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Partidos = await _context.PartidosPoliticos
                    .Where(x => x.Activo)
                    .OrderBy(x => x.Nombre)
                    .ToListAsync();

                return View(model);
            }

            // TEMPORAL
            // Luego sustituiremos esto por el partido del dirigente logueado
            int partidoSolicitanteId = 1;

            if (partidoSolicitanteId == model.PartidoAliadoId)
            {
                ModelState.AddModelError(string.Empty,
                    "Un partido no puede crear una alianza consigo mismo.");

                ViewBag.Partidos = await _context.PartidosPoliticos
                    .Where(x => x.Activo)
                    .ToListAsync();

                return View(model);
            }

            var existe = await _context.AlianzasPoliticas
                .AnyAsync(x =>
                    x.PartidoSolicitanteId == partidoSolicitanteId &&
                    x.PartidoAliadoId == model.PartidoAliadoId);

            if (existe)
            {
                ModelState.AddModelError(string.Empty,
                    "Ya existe una alianza registrada con este partido.");

                ViewBag.Partidos = await _context.PartidosPoliticos
                    .Where(x => x.Activo)
                    .ToListAsync();

                return View(model);
            }

            var alianza = new AlianzaPolitica
            {
                PartidoSolicitanteId = partidoSolicitanteId,
                PartidoAliadoId = model.PartidoAliadoId,
                FechaSolicitud = DateTime.Now,
                Estado = EstadoAlianza.Pendiente
            };

            _context.AlianzasPoliticas.Add(alianza);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: AlianzasPoliticas/Aceptar/5
        public async Task<IActionResult> Aceptar(int id)
        {
            var alianza = await _context.AlianzasPoliticas
                .FirstOrDefaultAsync(x => x.Id == id);

            if (alianza == null)
                return NotFound();

            alianza.Estado = EstadoAlianza.Aceptada;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: AlianzasPoliticas/Rechazar/5
        public async Task<IActionResult> Rechazar(int id)
        {
            var alianza = await _context.AlianzasPoliticas
                .FirstOrDefaultAsync(x => x.Id == id);

            if (alianza == null)
                return NotFound();

            alianza.Estado = EstadoAlianza.Rechazada;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: AlianzasPoliticas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var alianza = await _context.AlianzasPoliticas
                .Include(x => x.PartidoSolicitante)
                .Include(x => x.PartidoAliado)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (alianza == null)
                return NotFound();

            return View(alianza);
        }
    }
}