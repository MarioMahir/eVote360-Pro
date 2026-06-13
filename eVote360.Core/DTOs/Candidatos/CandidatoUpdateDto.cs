using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.DTOs.Candidatos;

public class CandidatoUpdateDto
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    public IFormFile? Foto { get; set; }

    public string FotoActual { get; set; } = string.Empty;

    public bool Activo { get; set; }
}