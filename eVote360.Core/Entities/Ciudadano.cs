using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Entities
{
    public class Ciudadano
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
