using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IPuestoElectivoService
{
    Task<List<PuestoElectivo>> GetAllAsync();

    Task<PuestoElectivo?> GetByIdAsync(int id);

    Task<(bool Success, string Error)> CreateAsync(
        PuestoElectivo puesto);

    Task<(bool Success, string Error)> UpdateAsync(
        PuestoElectivo puesto);

    Task<(bool Success, string Error)> ActivarAsync(int id);

    Task<(bool Success, string Error)> DesactivarAsync(int id);
}