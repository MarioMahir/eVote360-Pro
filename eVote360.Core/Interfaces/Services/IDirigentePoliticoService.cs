using eVote360.Core.DTOs.DirigentesPoliticos;
using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IDirigentePoliticoService
{
    Task<List<DirigentePolitico>> GetAllAsync();

    Task<List<Usuario>> GetDirigentesDisponiblesAsync();

    Task<List<PartidoPolitico>> GetPartidosDisponiblesAsync();

    Task<(bool Success, string Error)> CreateAsync(
        DirigentePoliticoCreateDto dto);

    Task<(bool Success, string Error)> DeleteAsync(int id);

    Task<DirigentePolitico?> GetByIdAsync(int id);
}