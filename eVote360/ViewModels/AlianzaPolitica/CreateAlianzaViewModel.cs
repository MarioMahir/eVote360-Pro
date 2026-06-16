using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels.AlianzaPolitica;

public class CreateAlianzaViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un partido aliado.")]
    public int PartidoAliadoId { get; set; }
}