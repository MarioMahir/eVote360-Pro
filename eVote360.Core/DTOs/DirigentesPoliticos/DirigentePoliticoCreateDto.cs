using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.DTOs.DirigentesPoliticos;

public class DirigentePoliticoCreateDto
{
    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public int PartidoPoliticoId { get; set; }
}