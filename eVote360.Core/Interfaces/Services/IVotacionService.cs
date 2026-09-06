using eVote360.Core.DTOs.Votacion;

namespace eVote360.Core.Interfaces.Services;

public interface IVotacionService
{
    /// <summary>Valida documento, elección activa, ciudadano activo y que no haya votado.</summary>
    Task<(bool Success, string Error, int CiudadanoId, int EleccionId)> IniciarAsync(string numeroDocumento);

    /// <summary>Genera un código de 6 dígitos con 5 minutos de vigencia y lo envía por correo.</summary>
    Task<(bool Success, string Error)> GenerarYEnviarCodigoAsync(int ciudadanoId, int eleccionId);

    Task<(bool Success, string Error)> ValidarCodigoAsync(int ciudadanoId, int eleccionId, string codigo);

    Task<List<PuestoBoletaDto>> GetPuestosBoletaAsync(int eleccionId, IReadOnlyDictionary<int, int?> selecciones);

    Task<List<OpcionBoletaDto>> GetOpcionesPuestoAsync(int eleccionId, int puestoElectivoId);

    Task<bool> PuestoPerteneceAEleccionAsync(int eleccionId, int puestoElectivoId);

    /// <summary>Registra los votos, marca la participación y envía el resumen por correo.</summary>
    Task<(bool Success, string Error, ResumenVotacionDto? Resumen)> FinalizarAsync(
        int ciudadanoId,
        int eleccionId,
        IReadOnlyDictionary<int, int?> selecciones);
}
