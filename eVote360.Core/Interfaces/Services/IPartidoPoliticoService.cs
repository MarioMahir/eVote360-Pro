using eVote360.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace eVote360.Core.Interfaces.Services;

public interface IPartidoPoliticoService
{
    Task<List<PartidoPolitico>> GetAllAsync();

    Task<PartidoPolitico?> GetByIdAsync(int id);

    Task<(bool Success, string Error)> CreateAsync(
        PartidoPolitico partido,
        IFormFile logo);

    Task<(bool Success, string Error)> UpdateAsync(
        PartidoPolitico partido,
        IFormFile? logo);

    Task<(bool Success, string Error)> ActivarAsync(int id);

    Task<(bool Success, string Error)> DesactivarAsync(int id);
}