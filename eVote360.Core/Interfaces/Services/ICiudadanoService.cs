using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface ICiudadanoService
{
    Task<List<Ciudadano>> GetAllAsync();
    Task<Ciudadano?> GetByIdAsync(int id);

    Task<(bool Success, string Error)> CreateAsync(Ciudadano ciudadano);

    Task<(bool Success, string Error)> UpdateAsync(Ciudadano ciudadano);

    Task<(bool Success, string Error)> ActivarAsync(int id);

    Task<(bool Success, string Error)> DesactivarAsync(int id);
}