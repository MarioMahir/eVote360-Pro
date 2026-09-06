using eVote360.Core.Entities;

namespace eVote360.Core.Interfaces.Services;

public interface IAuthService
{
    Task<(bool Success, string Error, Usuario? Usuario)>LoginAsync(string nombreUsuario, string password);
}