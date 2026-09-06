using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Repositories;
using eVote360.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class CiudadanoService : ICiudadanoService
{
    private readonly IGenericRepository<Ciudadano> _ciudadanos;
    private readonly IEleccionService _elecciones;

    public CiudadanoService(IGenericRepository<Ciudadano> ciudadanos, IEleccionService elecciones)
    {
        _ciudadanos = ciudadanos;
        _elecciones = elecciones;
    }

    public Task<List<Ciudadano>> GetAllAsync() =>
        _ciudadanos.Query().OrderBy(c => c.Apellido).ThenBy(c => c.Nombre).ToListAsync();

    public Task<Ciudadano?> GetByIdAsync(int id) => _ciudadanos.GetByIdAsync(id);

    public async Task<(bool Success, string Error)> CreateAsync(Ciudadano ciudadano)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede crear un ciudadano mientras exista una elección activa.");

        Normalizar(ciudadano);

        if (await _ciudadanos.AnyAsync(x => x.CorreoElectronico == ciudadano.CorreoElectronico))
            return (false, "Ya existe un ciudadano registrado con este correo electrónico.");

        if (await _ciudadanos.AnyAsync(x => x.NumeroDocumento == ciudadano.NumeroDocumento))
            return (false, "Ya existe un ciudadano registrado con este número de documento de identidad.");

        await _ciudadanos.AddAsync(ciudadano);
        await _ciudadanos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(Ciudadano ciudadano)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede editar un ciudadano mientras exista una elección activa.");

        var ciudadanoDb = await _ciudadanos.GetByIdAsync(ciudadano.Id);

        if (ciudadanoDb == null)
            return (false, "Ciudadano no encontrado.");

        Normalizar(ciudadano);

        if (ciudadanoDb.NumeroDocumento != ciudadano.NumeroDocumento &&
            await _elecciones.CiudadanoParticipoAsync(ciudadano.Id))
        {
            return (false, "No se puede modificar el número de documento de identidad de este ciudadano porque ya participó en una elección.");
        }

        if (await _ciudadanos.AnyAsync(x => x.CorreoElectronico == ciudadano.CorreoElectronico && x.Id != ciudadano.Id))
            return (false, "Ya existe un ciudadano registrado con este correo electrónico.");

        if (await _ciudadanos.AnyAsync(x => x.NumeroDocumento == ciudadano.NumeroDocumento && x.Id != ciudadano.Id))
            return (false, "Ya existe un ciudadano registrado con este número de documento de identidad.");

        ciudadanoDb.Nombre = ciudadano.Nombre;
        ciudadanoDb.Apellido = ciudadano.Apellido;
        ciudadanoDb.CorreoElectronico = ciudadano.CorreoElectronico;
        ciudadanoDb.NumeroDocumento = ciudadano.NumeroDocumento;
        ciudadanoDb.Activo = ciudadano.Activo;

        await _ciudadanos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede activar un ciudadano mientras exista una elección activa.");

        var ciudadano = await _ciudadanos.GetByIdAsync(id);

        if (ciudadano == null)
            return (false, "Ciudadano no encontrado.");

        if (ciudadano.Activo)
            return (false, "Este ciudadano ya se encuentra activo.");

        if (await _ciudadanos.AnyAsync(x => x.Id != id && x.NumeroDocumento == ciudadano.NumeroDocumento))
            return (false, "Existe otro ciudadano con el mismo número de documento.");

        ciudadano.Activo = true;
        await _ciudadanos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede desactivar un ciudadano mientras exista una elección activa.");

        var ciudadano = await _ciudadanos.GetByIdAsync(id);

        if (ciudadano == null)
            return (false, "Ciudadano no encontrado.");

        if (!ciudadano.Activo)
            return (false, "Este ciudadano ya se encuentra inactivo.");

        ciudadano.Activo = false;
        await _ciudadanos.SaveChangesAsync();

        return (true, string.Empty);
    }

    private static void Normalizar(Ciudadano c)
    {
        c.Nombre = c.Nombre.Trim();
        c.Apellido = c.Apellido.Trim();
        c.CorreoElectronico = c.CorreoElectronico.Trim().ToLowerInvariant();
        c.NumeroDocumento = c.NumeroDocumento.Trim();
    }
}
