using eVote360.Core.DTOs.Candidatos;
using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface ICandidatoService
{
    Task<List<Candidato>> GetAllAsync(int partidoId);

    Task<Candidato?> GetByIdAsync(int id, int partidoId);

    Task<(bool Success, string Error)> CreateAsync(CandidatoCreateDto dto, int partidoId);

    Task<(bool Success, string Error)> UpdateAsync(CandidatoUpdateDto dto, int partidoId);

    Task<(bool Success, string Error)> ActivarAsync(int id, int partidoId);

    Task<(bool Success, string Error)> DesactivarAsync(int id, int partidoId);
}