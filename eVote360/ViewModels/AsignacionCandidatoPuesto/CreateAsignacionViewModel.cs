using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels.AsignacionCandidatoPuesto;

public class CreateAsignacionViewModel
{
    [Required]
    public int EleccionId { get; set; }

    [Required]
    public int CandidatoId { get; set; }

    [Required]
    public int PuestoElectivoId { get; set; }
}