using System.ComponentModel.DataAnnotations;
using eVote360.Core.DTOs.Elecciones;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.ViewModels.Admin;

public class EleccionCreateViewModel
{
    [Required(ErrorMessage = "El nombre de la elección es requerido.")]
    [MaxLength(150)]
    [Display(Name = "Nombre de la elección")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de realización es requerida.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de realización")]
    public DateTime? FechaEleccion { get; set; }
}

public class ResumenAnioViewModel
{
    [Display(Name = "Año electoral")]
    public int? Anio { get; set; }

    public List<SelectListItem> Anios { get; set; } = [];

    public List<ResumenEleccionDto>? Resultados { get; set; }

    public bool SinElecciones => Anios.Count == 0;
}

public class PuestoElectivoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del puesto es requerido.")]
    [MaxLength(100)]
    [Display(Name = "Nombre del puesto")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es requerida.")]
    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public bool NombreBloqueado { get; set; }
}

public class CiudadanoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
    [MaxLength(150)]
    [Display(Name = "Correo electrónico")]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de documento de identidad es requerido.")]
    [MaxLength(20)]
    [Display(Name = "Número de documento de identidad")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public bool DocumentoBloqueado { get; set; }
}

public class PartidoPoliticoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del partido es requerido.")]
    [MaxLength(150)]
    [Display(Name = "Nombre del partido")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Las siglas son requeridas.")]
    [MaxLength(20)]
    public string Siglas { get; set; } = string.Empty;

    [Display(Name = "Logo del partido")]
    public IFormFile? Logo { get; set; }

    public string? LogoActual { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public bool DatosBloqueados { get; set; }
}
