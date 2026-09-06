using System.ComponentModel.DataAnnotations;
using eVote360.Core.DTOs.Votacion;

namespace eVote360.ViewModels.Elector;

public class DocumentoViewModel
{
    [Required(ErrorMessage = "El número de documento de identidad es requerido.")]
    [Display(Name = "Número de documento de identidad")]
    public string NumeroDocumento { get; set; } = string.Empty;
}

public class CedulaViewModel
{
    [Display(Name = "Imagen de la cédula")]
    public IFormFile? Imagen { get; set; }
}

public class CodigoViewModel
{
    [Required(ErrorMessage = "Debe ingresar el código de verificación enviado a su correo electrónico.")]
    [Display(Name = "Código de verificación")]
    public string Codigo { get; set; } = string.Empty;

    public string CorreoOculto { get; set; } = string.Empty;
}

public class VotoPuestoViewModel
{
    public int PuestoElectivoId { get; set; }

    public string PuestoNombre { get; set; } = string.Empty;

    /// <summary>Id del candidato, o 0 para la opción "Ninguno".</summary>
    public int? CandidatoSeleccionado { get; set; }

    public List<OpcionBoletaDto> Opciones { get; set; } = [];
}
