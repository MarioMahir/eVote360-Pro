using eVote360.Core.DTOs.Elecciones;
using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IEleccionService
{
    Task<List<EleccionListDto>> GetAllAsync();

    Task<Eleccion?> GetByIdAsync(int id);

    Task<bool> ExisteEleccionActivaAsync();

    Task<Eleccion?> GetEleccionActivaAsync();

    /// <summary>Valida la configuración electoral actual. Devuelve la lista de problemas (vacía si es válida).</summary>
    Task<List<string>> ValidarConfiguracionAsync();

    Task<(bool Success, string Error)> CreateAsync(string nombre, DateTime fechaEleccion);

    Task<(bool Success, string Error)> ActivarAsync(int id);

    Task<(bool Success, string Error)> FinalizarAsync(int id);

    Task<List<ResultadoPuestoDto>?> GetResultadosAsync(int id);

    Task<List<int>> GetAniosConEleccionesAsync();

    Task<List<ResumenEleccionDto>> GetResumenPorAnioAsync(int anio);

    // Participación en elecciones activas o finalizadas (bloqueo de campos críticos)
    Task<bool> PuestoParticipoAsync(int puestoId);

    Task<bool> PartidoParticipoAsync(int partidoId);

    Task<bool> CandidatoParticipoAsync(int candidatoId);

    Task<bool> CiudadanoParticipoAsync(int ciudadanoId);
}
