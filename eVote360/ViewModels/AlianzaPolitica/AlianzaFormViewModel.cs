using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels.AlianzaPolitica;

public class AlianzaFormViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un partido.")]
    public int PartidoAliadoId { get; set; }

    public List<SelectListItem> Partidos { get; set; } = [];
}
