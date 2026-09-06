using System.Security.Cryptography;
using System.Text;

namespace eVote360.Infrastructure.Security;

/// <summary>
/// Hash de contraseñas compartido por el registro de usuarios y el inicio de sesión.
/// Mantiene el esquema SHA-256/Base64 con el que se crearon los usuarios existentes.
/// </summary>
public static class PasswordHasher
{
    public static string Hash(string texto)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        return Convert.ToBase64String(bytes);
    }

    public static bool Verify(string texto, string hash) => Hash(texto) == hash;
}
