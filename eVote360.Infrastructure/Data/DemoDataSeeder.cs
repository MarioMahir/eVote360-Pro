using eVote360.Core.Constants;
using eVote360.Core.Entities;
using eVote360.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Data;

/// <summary>
/// Datos de demostración para probar el sistema de punta a punta
/// (se cargan solo si la base no tiene partidos). Activar con "SeedDemoData": true.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.PartidosPoliticos.AnyAsync())
            return;

        // Puestos electivos
        var puestos = new List<PuestoElectivo>
        {
            new() { Nombre = "Presidente", Descripcion = "Jefe de Estado y de Gobierno de la República." },
            new() { Nombre = "Senador", Descripcion = "Cargo legislativo que representa una provincia en el Senado." },
            new() { Nombre = "Alcalde", Descripcion = "Cargo municipal encargado de la administración del ayuntamiento." }
        };
        context.PuestosElectivos.AddRange(puestos);

        // Partidos políticos
        var partidos = new List<PartidoPolitico>
        {
            new() { Nombre = "Partido Nacional Democrático", Siglas = "PND", Descripcion = "Organización política orientada a la participación democrática nacional.", LogoUrl = "/uploads/demo/logo-pnd.png" },
            new() { Nombre = "Movimiento de Unidad Ciudadana", Siglas = "MUC", Descripcion = "Partido político con representación municipal y congresual.", LogoUrl = "/uploads/demo/logo-muc.png" },
            new() { Nombre = "Partido Popular Reformista", Siglas = "PPR", Descripcion = "Movimiento reformista de base popular.", LogoUrl = "/uploads/demo/logo-ppr.png" },
            new() { Nombre = "Frente Progresista Verde", Siglas = "FPV", Descripcion = "Coalición con enfoque ambiental y social.", LogoUrl = "/uploads/demo/logo-fpv.png" }
        };
        context.PartidosPoliticos.AddRange(partidos);

        // Usuarios dirigentes (contraseña: Dirigente1234)
        var hash = PasswordHasher.Hash("Dirigente1234");
        var dirigentes = new List<Usuario>
        {
            new() { Nombre = "María", Apellido = "Rodríguez", CorreoElectronico = "mrodriguez@pnd.do", NombreUsuario = "mrodriguez", PasswordHash = hash, Rol = Roles.DirigentePolitico },
            new() { Nombre = "Carlos", Apellido = "Pérez", CorreoElectronico = "cperez@muc.do", NombreUsuario = "cperez", PasswordHash = hash, Rol = Roles.DirigentePolitico },
            new() { Nombre = "Laura", Apellido = "Gómez", CorreoElectronico = "lgomez@ppr.do", NombreUsuario = "lgomez", PasswordHash = hash, Rol = Roles.DirigentePolitico },
            new() { Nombre = "José", Apellido = "Martínez", CorreoElectronico = "jmartinez@fpv.do", NombreUsuario = "jmartinez", PasswordHash = hash, Rol = Roles.DirigentePolitico }
        };
        context.Usuarios.AddRange(dirigentes);

        // Ciudadanos (el 00112345678 coincide con docs/cedula-prueba.png)
        var ciudadanos = new List<Ciudadano>
        {
            new() { Nombre = "Juan", Apellido = "Pérez Rodríguez", CorreoElectronico = "juan.perez@email.com", NumeroDocumento = "00112345678" },
            new() { Nombre = "Ana", Apellido = "Martínez", CorreoElectronico = "ana.martinez@email.com", NumeroDocumento = "40212345678" },
            new() { Nombre = "Pedro", Apellido = "Gómez", CorreoElectronico = "pedro.gomez@email.com", NumeroDocumento = "00198765432" },
            new() { Nombre = "Lucía", Apellido = "Fernández", CorreoElectronico = "lucia.fernandez@email.com", NumeroDocumento = "40287654321" },
            new() { Nombre = "Roberto", Apellido = "Santos", CorreoElectronico = "roberto.santos@email.com", NumeroDocumento = "00155566677", Activo = false }
        };
        context.Ciudadanos.AddRange(ciudadanos);

        await context.SaveChangesAsync();

        // Relación dirigente ↔ partido
        for (var i = 0; i < 4; i++)
            context.DirigentesPoliticos.Add(new DirigentePolitico { UsuarioId = dirigentes[i].Id, PartidoPoliticoId = partidos[i].Id });

        // Candidatos: 3 por partido (uno por puesto)
        string[][] nombres =
        [
            ["Juan|Pérez", "María|Gómez", "Carlos|Reyes"],
            ["Andrés|Mena", "Luisa|Ferrer", "Ramón|Soto"],
            ["Patricia|Díaz", "Víctor|Herrera", "Elena|Morales"],
            ["Diego|Castillo", "Sofía|Luna", "Tomás|Báez"]
        ];

        var foto = 1;
        var candidatos = new List<Candidato>();

        for (var p = 0; p < 4; p++)
        {
            for (var q = 0; q < 3; q++)
            {
                var partes = nombres[p][q].Split('|');
                candidatos.Add(new Candidato
                {
                    Nombre = partes[0],
                    Apellido = partes[1],
                    FotoUrl = $"/uploads/demo/foto-{foto++}.png",
                    PartidoPoliticoId = partidos[p].Id,
                    Activo = true
                });
            }
        }

        context.Candidatos.AddRange(candidatos);
        await context.SaveChangesAsync();

        // Asignaciones: cada partido cubre los 3 puestos con candidatos propios
        for (var p = 0; p < 4; p++)
        {
            for (var q = 0; q < 3; q++)
            {
                context.AsignacionesCandidatoPuesto.Add(new AsignacionCandidatoPuesto
                {
                    PartidoPoliticoId = partidos[p].Id,
                    CandidatoId = candidatos[p * 3 + q].Id,
                    PuestoElectivoId = puestos[q].Id
                });
            }
        }

        // Alianza vigente PND ↔ MUC y una solicitud pendiente de PPR hacia FPV
        context.AlianzasPoliticas.Add(new AlianzaPolitica
        {
            PartidoSolicitanteId = partidos[0].Id,
            PartidoAliadoId = partidos[1].Id,
            Estado = EstadoAlianza.Aceptada,
            Vigente = true,
            FechaSolicitud = DateTime.Now.AddDays(-10),
            FechaRespuesta = DateTime.Now.AddDays(-9)
        });

        context.AlianzasPoliticas.Add(new AlianzaPolitica
        {
            PartidoSolicitanteId = partidos[2].Id,
            PartidoAliadoId = partidos[3].Id,
            Estado = EstadoAlianza.Pendiente,
            FechaSolicitud = DateTime.Now.AddDays(-2)
        });

        await context.SaveChangesAsync();
    }
}
