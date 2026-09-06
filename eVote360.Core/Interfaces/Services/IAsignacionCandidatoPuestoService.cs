using eVote360.Core.DTOs.Asignaciones;
using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IAsignacionCandidatoPuestoService
{
    Task<List<AsignacionListDto>> GetAllAsync(int partidoId);

    Task<AsignacionCandidatoPuesto?> GetByIdAsync(int id, int partidoId);

    Task<List<CandidatoDisponibleDto>> GetCandidatosDisponiblesAsync(int partidoId);

    Task<List<PuestoElectivo>> GetPuestosDisponiblesAsync(int partidoId);

    Task<(bool Success, string Error)> CreateAsync(int partidoId, int candidatoId, int puestoElectivoId);

    Task<(bool Success, string Error)> DeleteAsync(int id, int partidoId);
}
