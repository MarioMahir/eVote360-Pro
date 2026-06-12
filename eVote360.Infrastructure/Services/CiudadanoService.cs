using eVote360.Core.Interfaces;
using eVote360.Infrastructure.Data;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Services;

public class CiudadanoService : ICiudadanoService
{
    private readonly AppDbContext _context;

    public CiudadanoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ciudadano>> GetAllAsync()
    {
        return await _context.Ciudadanos.ToListAsync();
    }

    public async Task<Ciudadano?> GetByIdAsync(int id)
    {
        return await _context.Ciudadanos.FindAsync(id);
    }

    public async Task<(bool Success, string Error)> CreateAsync(
        Ciudadano ciudadano)
    {
        ciudadano.NumeroDocumento =
            ciudadano.NumeroDocumento.Trim();

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.CorreoElectronico == ciudadano.CorreoElectronico))
        {
            return (false,
                "Ya existe un ciudadano registrado con este correo electrónico.");
        }

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.NumeroDocumento == ciudadano.NumeroDocumento))
        {
            return (false,
                "Ya existe un ciudadano registrado con este número de documento de identidad.");
        }

        _context.Ciudadanos.Add(ciudadano);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(
        Ciudadano ciudadano)
    {
        ciudadano.NumeroDocumento =
            ciudadano.NumeroDocumento.Trim();

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.CorreoElectronico == ciudadano.CorreoElectronico &&
            x.Id != ciudadano.Id))
        {
            return (false,
                "Ya existe un ciudadano registrado con este correo electrónico.");
        }

        if (await _context.Ciudadanos.AnyAsync(x =>
            x.NumeroDocumento == ciudadano.NumeroDocumento &&
            x.Id != ciudadano.Id))
        {
            return (false,
                "Ya existe un ciudadano registrado con este número de documento de identidad.");
        }

        _context.Update(ciudadano);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        var ciudadano =
            await _context.Ciudadanos.FindAsync(id);

        if (ciudadano == null)
            return (false, "Ciudadano no encontrado.");

        if (ciudadano.Activo)
            return (false,
                "Este ciudadano ya se encuentra activo.");

        ciudadano.Activo = true;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id)
    {
        var ciudadano =
            await _context.Ciudadanos.FindAsync(id);

        if (ciudadano == null)
            return (false, "Ciudadano no encontrado.");

        if (!ciudadano.Activo)
            return (false,
                "Este ciudadano ya se encuentra inactivo.");

        ciudadano.Activo = false;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}