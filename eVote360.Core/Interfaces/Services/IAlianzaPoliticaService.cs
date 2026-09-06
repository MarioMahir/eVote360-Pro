using eVote360.Core.DTOs.Alianzas;
using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IAlianzaPoliticaService
{
    Task<List<SolicitudAlianzaDto>> GetPendientesRecibidasAsync(int partidoId);

    Task<List<SolicitudAlianzaDto>> GetRealizadasAsync(int partidoId);

    Task<List<AlianzaVigenteDto>> GetVigentesAsync(int partidoId);

    Task<List<PartidoPolitico>> GetPartidosDisponiblesAsync(int partidoId);

    Task<AlianzaPolitica?> GetByIdAsync(int id);

    Task<(bool Success, string Error)> CrearSolicitudAsync(int partidoId, int partidoAliadoId);

    Task<(bool Success, string Error)> AceptarAsync(int id, int partidoId);

    Task<(bool Success, string Error)> RechazarAsync(int id, int partidoId);

    Task<(bool Success, string Error)> EliminarSolicitudAsync(int id, int partidoId);

    Task<(bool Success, string Error)> EliminarAlianzaAsync(int id, int partidoId);

    Task<bool> ExisteAlianzaVigenteAsync(int partidoA, int partidoB);
}
