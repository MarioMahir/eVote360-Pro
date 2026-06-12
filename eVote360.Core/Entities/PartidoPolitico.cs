using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Entities
{
    public class PartidoPolitico
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [MaxLength(20)]
        public string Siglas { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}