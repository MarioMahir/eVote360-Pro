using eVote360.Core.Entities;
using eVote360.Core.DTOs.Usuarios;

namespace eVote360.Core.Interfaces.Services;

public interface IUsuarioService
{
    Task<List<Usuario>> GetAllAsync();

    Task<Usuario?> GetByIdAsync(int id);

    Task<(bool Success, string Error)> CreateAsync(UsuarioCreateDto dto);

    Task<(bool Success, string Error)> UpdateAsync(UsuarioUpdateDto dto);

    Task<(bool Success, string Error)> ActivarAsync(int id);

    Task<(bool Success, string Error)> DesactivarAsync(int id);
}