using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.DTOs.Candidatos;

public class CandidatoCreateDto
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    public IFormFile Foto { get; set; } = null!;

    public bool Activo { get; set; } = true;
}