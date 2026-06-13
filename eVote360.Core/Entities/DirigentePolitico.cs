using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Entities;

public class DirigentePolitico
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int PartidoPoliticoId { get; set; }

    public PartidoPolitico PartidoPolitico { get; set; } = null!;
}