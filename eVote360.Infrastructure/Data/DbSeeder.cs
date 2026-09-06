using eVote360.Core.Constants;
using eVote360.Core.Entities;
using eVote360.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Data;

public static class DbSeeder
{
    /// <summary>Crea el administrador inicial si no existe ninguno. Usuario: admin / Contraseña: Admin1234</summary>
    public static async Task SeedAdministradorAsync(AppDbContext context)
    {
        if (await context.Usuarios.AnyAsync(u => u.Rol == Roles.Administrador))
            return;

        context.Usuarios.Add(new Usuario
        {
            Nombre = "Administrador",
            Apellido = "del Sistema",
            CorreoElectronico = "admin@evote360.local",
            NombreUsuario = "admin",
            PasswordHash = PasswordHasher.Hash("Admin1234"),
            Rol = Roles.Administrador,
            Activo = true
        });

        await context.SaveChangesAsync();
    }
}
