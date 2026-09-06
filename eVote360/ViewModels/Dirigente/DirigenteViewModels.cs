using System.ComponentModel.DataAnnotations;
using eVote360.Core.DTOs.Alianzas;
using eVote360.Core.DTOs.Dirigente;
using eVote360.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.ViewModels.Dirigente;

public class HomeDirigenteViewModel
{
    public PartidoPolitico Partido { get; set; } = null!;

    public IndicadoresDirigenteDto Indicadores { get; set; } = new();

    public bool ExisteEleccionActiva { get; set; }
}

public class AlianzasIndexViewModel
{
    public List<SolicitudAlianzaDto> PendientesDeResponder { get; set; } = [];

    public List<SolicitudAlianzaDto> Realizadas { get; set; } = [];

    public List<AlianzaVigenteDto> Vigentes { get; set; } = [];

    public bool ExisteEleccionActiva { get; set; }
}

public class AlianzaCreateViewModel
{
    [Required(ErrorMessage = "El partido político es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El partido político es requerido.")]
    [Display(Name = "Partido político")]
    public int PartidoAliadoId { get; set; }

    public List<SelectListItem> Partidos { get; set; } = [];
}

public class AsignacionCreateViewModel
{
    [Required(ErrorMessage = "El candidato político es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El candidato político es requerido.")]
    [Display(Name = "Candidato político")]
    public int CandidatoId { get; set; }

    [Required(ErrorMessage = "El puesto electivo es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El puesto electivo es requerido.")]
    [Display(Name = "Puesto electivo")]
    public int PuestoElectivoId { get; set; }

    public List<SelectListItem> Candidatos { get; set; } = [];

    public List<SelectListItem> Puestos { get; set; } = [];
}
