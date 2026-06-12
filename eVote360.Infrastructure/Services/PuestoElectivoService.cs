using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class PuestoElectivoService : IPuestoElectivoService
{
    private readonly AppDbContext _context;

    public PuestoElectivoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PuestoElectivo>> GetAllAsync()
    {
        return await _context.PuestosElectivos.ToListAsync();
    }

    public async Task<PuestoElectivo?> GetByIdAsync(int id)
    {
        return await _context.PuestosElectivos.FindAsync(id);
    }

    public async Task<(bool Success, string Error)> CreateAsync(
        PuestoElectivo puesto)
    {
        puesto.Nombre = puesto.Nombre.Trim();

        bool existe =
            await _context.PuestosElectivos
            .AnyAsync(x => x.Nombre == puesto.Nombre);

        if (existe)
        {
            return (
                false,
                "Ya existe un puesto electivo registrado con este nombre.");
        }

        _context.PuestosElectivos.Add(puesto);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(
        PuestoElectivo puesto)
    {
        puesto.Nombre = puesto.Nombre.Trim();

        bool existe =
            await _context.PuestosElectivos
            .AnyAsync(x =>
                x.Id != puesto.Id &&
                x.Nombre == puesto.Nombre);

        if (existe)
        {
            return (
                false,
                "Ya existe un puesto electivo registrado con este nombre.");
        }

        _context.PuestosElectivos.Update(puesto);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        var puesto =
            await _context.PuestosElectivos.FindAsync(id);

        if (puesto == null)
            return (false, "Puesto no encontrado.");

        if (puesto.Activo)
        {
            return (
                false,
                "Este puesto electivo ya se encuentra activo.");
        }

        puesto.Activo = true;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id)
    {
        var puesto =
            await _context.PuestosElectivos.FindAsync(id);

        if (puesto == null)
            return (false, "Puesto no encontrado.");

        if (!puesto.Activo)
        {
            return (
                false,
                "Este puesto electivo ya se encuentra inactivo.");
        }

        puesto.Activo = false;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}