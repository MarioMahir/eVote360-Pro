using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels.AsignacionCandidatoPuesto;

public class AsignacionFormViewModel
{
    [Required]
    public int EleccionId { get; set; }

    [Required]
    public int CandidatoId { get; set; }

    [Required]
    public int PuestoElectivoId { get; set; }

    public List<SelectListItem> Elecciones { get; set; } = [];

    public List<SelectListItem> Candidatos { get; set; } = [];

    public List<SelectListItem> PuestosElectivos { get; set; } = [];
}