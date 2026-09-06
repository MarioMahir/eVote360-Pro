using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Repositories;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class PuestoElectivoService : IPuestoElectivoService
{
    private readonly IGenericRepository<PuestoElectivo> _puestos;
    private readonly AppDbContext _context;
    private readonly IEleccionService _elecciones;

    public PuestoElectivoService(
        IGenericRepository<PuestoElectivo> puestos,
        AppDbContext context,
        IEleccionService elecciones)
    {
        _puestos = puestos;
        _context = context;
        _elecciones = elecciones;
    }

    public Task<List<PuestoElectivo>> GetAllAsync() =>
        _puestos.Query().OrderBy(p => p.Nombre).ToListAsync();

    public Task<PuestoElectivo?> GetByIdAsync(int id) => _puestos.GetByIdAsync(id);

    public async Task<(bool Success, string Error)> CreateAsync(PuestoElectivo puesto)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede crear un puesto electivo mientras exista una elección activa.");

        puesto.Nombre = puesto.Nombre.Trim();
        puesto.Descripcion = puesto.Descripcion.Trim();

        if (await _puestos.AnyAsync(x => x.Nombre == puesto.Nombre))
            return (false, "Ya existe un puesto electivo registrado con este nombre.");

        await _puestos.AddAsync(puesto);
        await _puestos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(PuestoElectivo puesto)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede editar un puesto electivo mientras exista una elección activa.");

        var puestoDb = await _puestos.GetByIdAsync(puesto.Id);

        if (puestoDb == null)
            return (false, "Puesto electivo no encontrado.");

        puesto.Nombre = puesto.Nombre.Trim();
        puesto.Descripcion = puesto.Descripcion.Trim();

        if (puestoDb.Nombre != puesto.Nombre && await _elecciones.PuestoParticipoAsync(puesto.Id))
            return (false, "No se puede modificar el nombre de este puesto electivo porque ya fue utilizado en una elección.");

        if (await _puestos.AnyAsync(x => x.Id != puesto.Id && x.Nombre == puesto.Nombre))
            return (false, "Ya existe un puesto electivo registrado con este nombre.");

        if (!puesto.Activo && puestoDb.Activo && await TieneCandidatosAsignadosAsync(puesto.Id))
            return (false, "No se puede desactivar este puesto electivo porque tiene candidatos asignados.");

        puestoDb.Nombre = puesto.Nombre;
        puestoDb.Descripcion = puesto.Descripcion;
        puestoDb.Activo = puesto.Activo;

        await _puestos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede activar un puesto electivo mientras exista una elección activa.");

        var puesto = await _puestos.GetByIdAsync(id);

        if (puesto == null)
            return (false, "Puesto electivo no encontrado.");

        if (puesto.Activo)
            return (false, "Este puesto electivo ya se encuentra activo.");

        if (await _puestos.AnyAsync(x => x.Id != id && x.Nombre == puesto.Nombre))
            return (false, "Ya existe otro puesto electivo con el mismo nombre.");

        puesto.Activo = true;
        await _puestos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede desactivar un puesto electivo mientras exista una elección activa.");

        var puesto = await _puestos.GetByIdAsync(id);

        if (puesto == null)
            return (false, "Puesto electivo no encontrado.");

        if (!puesto.Activo)
            return (false, "Este puesto electivo ya se encuentra inactivo.");

        if (await TieneCandidatosAsignadosAsync(id))
            return (false, "No se puede desactivar este puesto electivo porque tiene candidatos activos asignados.");

        puesto.Activo = false;
        await _puestos.SaveChangesAsync();

        return (true, string.Empty);
    }

    private Task<bool> TieneCandidatosAsignadosAsync(int puestoId) =>
        _context.AsignacionesCandidatoPuesto
            .Include(a => a.Candidato)
            .AnyAsync(a => a.PuestoElectivoId == puestoId && a.Candidato.Activo);
}
