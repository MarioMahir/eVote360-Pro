using eVote360.Core.DTOs.Usuarios;
using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IUsuarioService
{
    Task<List<Usuario>> GetAllAsync();

    Task<Usuario?> GetByIdAsync(int id);

    Task<(bool Success, string Error)> CreateAsync(UsuarioCreateDto dto);

    /// <param name="usuarioActualId">Id del administrador autenticado, para impedir que cambie su propio rol o se desactive.</param>
    Task<(bool Success, string Error)> UpdateAsync(UsuarioUpdateDto dto, int usuarioActualId);

    Task<(bool Success, string Error)> ActivarAsync(int id);

    Task<(bool Success, string Error)> DesactivarAsync(int id, int usuarioActualId);
}
