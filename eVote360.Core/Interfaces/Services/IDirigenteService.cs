using eVote360.Core.DTOs.Dirigente;
using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IDirigenteService
{
    /// <summary>Partido asignado al usuario dirigente, o null si no tiene.</summary>
    Task<PartidoPolitico?> GetPartidoDeUsuarioAsync(int usuarioId);

    Task<IndicadoresDirigenteDto> GetIndicadoresAsync(int partidoId);
}
