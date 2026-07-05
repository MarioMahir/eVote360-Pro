using eVote360.Core.Entities;
using eVote360.Infrastructure.Data;
using eVote360.ViewModels.AsignacionCandidatoPuesto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Controllers
{
    public class AsignacionCandidatoPuestoController : Controller
    {
        private readonly AppDbContext _context;

        public AsignacionCandidatoPuestoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AsignacionCandidatoPuesto
        public async Task<IActionResult> Index()
        {
            var asignaciones = await _context.AsignacionesCandidatoPuesto
                .Include(x => x.Eleccion)
                .Include(x => x.Candidato)
                .Include(x => x.PuestoElectivo)
                .OrderByDescending(x => x.FechaAsignacion)
                .ToListAsync();

            return View(asignaciones);
        }

        // GET: AsignacionCandidatoPuesto/Create
        public async Task<IActionResult> Create()
        {
            var model = new AsignacionFormViewModel();

            await CargarCombos(model);

            return View(model);
        }

        // POST: AsignacionCandidatoPuesto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AsignacionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos(model);
                return View(model);
            }

            var eleccion = await _context.Elecciones
                .FirstOrDefaultAsync(x => x.Id == model.EleccionId);

            if (eleccion == null)
            {
                ModelState.AddModelError("", "La elección seleccionada no existe.");

                await CargarCombos(model);
                return View(model);
            }

            if (!eleccion.Activa)
            {
                ModelState.AddModelError("", "La elección seleccionada está inactiva.");

                await CargarCombos(model);
                return View(model);
            }

            var candidato = await _context.Candidatos
                .FirstOrDefaultAsync(x => x.Id == model.CandidatoId);

            if (candidato == null)
            {
                ModelState.AddModelError("", "El candidato seleccionado no existe.");

                await CargarCombos(model);
                return View(model);
            }

            if (!candidato.Activo)
            {
                ModelState.AddModelError("", "El candidato está inactivo.");

                await CargarCombos(model);
                return View(model);
            }

            var puesto = await _context.PuestosElectivos
                .FirstOrDefaultAsync(x => x.Id == model.PuestoElectivoId);

            if (puesto == null)
            {
                ModelState.AddModelError("", "El puesto electivo seleccionado no existe.");

                await CargarCombos(model);
                return View(model);
            }

            if (!puesto.Activo)
            {
                ModelState.AddModelError("", "El puesto electivo está inactivo.");

                await CargarCombos(model);
                return View(model);
            }

            var existe = await _context.AsignacionesCandidatoPuesto
                .AnyAsync(x =>
                    x.EleccionId == model.EleccionId &&
                    x.CandidatoId == model.CandidatoId &&
                    x.PuestoElectivoId == model.PuestoElectivoId);

            if (existe)
            {
                ModelState.AddModelError("", "Esta asignación ya existe.");

                await CargarCombos(model);
                return View(model);
            }

            var asignacion = new AsignacionCandidatoPuesto
            {
                EleccionId = model.EleccionId,
                CandidatoId = model.CandidatoId,
                PuestoElectivoId = model.PuestoElectivoId,
                FechaAsignacion = DateTime.Now,
                Activo = true
            };

            _context.AsignacionesCandidatoPuesto.Add(asignacion);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "La asignación fue creada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombos(AsignacionFormViewModel model)
        {
            model.Elecciones = await _context.Elecciones
                .Where(x => x.Activa)
                .OrderBy(x => x.Nombre)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Nombre
                })
                .ToListAsync();

            model.Candidatos = await _context.Candidatos
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Nombre + " " + x.Apellido
                })
                .ToListAsync();

            model.PuestosElectivos = await _context.PuestosElectivos
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Nombre
                })
                .ToListAsync();
        }
    }
}