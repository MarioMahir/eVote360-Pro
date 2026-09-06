using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Repositories;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class PartidoPoliticoService : IPartidoPoliticoService
{
    private readonly IGenericRepository<PartidoPolitico> _partidos;
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;
    private readonly IEleccionService _elecciones;

    public PartidoPoliticoService(
        IGenericRepository<PartidoPolitico> partidos,
        AppDbContext context,
        IImageService imageService,
        IEleccionService elecciones)
    {
        _partidos = partidos;
        _context = context;
        _imageService = imageService;
        _elecciones = elecciones;
    }

    public Task<List<PartidoPolitico>> GetAllAsync() =>
        _partidos.Query().OrderBy(p => p.Nombre).ToListAsync();

    public Task<PartidoPolitico?> GetByIdAsync(int id) => _partidos.GetByIdAsync(id);

    public async Task<(bool Success, string Error)> CreateAsync(PartidoPolitico partido, IFormFile logo)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede crear un partido político mientras exista una elección activa.");

        partido.Nombre = partido.Nombre.Trim();
        partido.Siglas = partido.Siglas.Trim().ToUpperInvariant();
        partido.Descripcion = partido.Descripcion?.Trim();

        if (await _partidos.AnyAsync(x => x.Siglas == partido.Siglas))
            return (false, "Ya existe un partido político registrado con estas siglas.");

        if (logo == null || logo.Length == 0)
            return (false, "El logo del partido es requerido.");

        if (!_imageService.IsValidImage(logo))
            return (false, "El logo del partido debe ser una imagen válida.");

        partido.LogoUrl = await _imageService.SavePartyLogoAsync(logo, partido.Siglas);

        await _partidos.AddAsync(partido);
        await _partidos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(PartidoPolitico partido, IFormFile? logo)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede editar un partido político mientras exista una elección activa.");

        var partidoDb = await _partidos.GetByIdAsync(partido.Id);

        if (partidoDb == null)
            return (false, "Partido político no encontrado.");

        partido.Nombre = partido.Nombre.Trim();
        partido.Siglas = partido.Siglas.Trim().ToUpperInvariant();
        partido.Descripcion = partido.Descripcion?.Trim();

        var participo = await _elecciones.PartidoParticipoAsync(partido.Id);

        if (participo && (partidoDb.Nombre != partido.Nombre || partidoDb.Siglas != partido.Siglas || logo != null))
            return (false, "No se pueden modificar los datos principales de este partido político porque ya participó en una elección.");

        if (await _partidos.AnyAsync(x => x.Id != partido.Id && x.Siglas == partido.Siglas))
            return (false, "Ya existe un partido político registrado con estas siglas.");

        if (!partido.Activo && partidoDb.Activo)
        {
            var bloqueo = await ValidarDesactivacionAsync(partido.Id);
            if (bloqueo != null) return (false, bloqueo);
        }

        if (logo != null)
        {
            if (!_imageService.IsValidImage(logo))
                return (false, "El logo del partido debe ser una imagen válida.");

            _imageService.DeleteImage(partidoDb.LogoUrl);
            partidoDb.LogoUrl = await _imageService.SavePartyLogoAsync(logo, partido.Siglas);
        }

        partidoDb.Nombre = partido.Nombre;
        partidoDb.Descripcion = partido.Descripcion;
        partidoDb.Siglas = partido.Siglas;
        partidoDb.Activo = partido.Activo;

        await _partidos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede activar un partido político mientras exista una elección activa.");

        var partido = await _partidos.GetByIdAsync(id);

        if (partido == null)
            return (false, "Partido político no encontrado.");

        if (partido.Activo)
            return (false, "Este partido político ya se encuentra activo.");

        if (await _partidos.AnyAsync(x => x.Id != id && x.Siglas == partido.Siglas))
            return (false, "Existe otro partido político con las mismas siglas.");

        partido.Activo = true;
        await _partidos.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede desactivar un partido político mientras exista una elección activa.");

        var partido = await _partidos.GetByIdAsync(id);

        if (partido == null)
            return (false, "Partido político no encontrado.");

        if (!partido.Activo)
            return (false, "Este partido político ya se encuentra inactivo.");

        var bloqueo = await ValidarDesactivacionAsync(id);
        if (bloqueo != null) return (false, bloqueo);

        partido.Activo = false;
        await _partidos.SaveChangesAsync();

        return (true, string.Empty);
    }

    private async Task<string?> ValidarDesactivacionAsync(int partidoId)
    {
        if (await _context.Candidatos.AnyAsync(c => c.PartidoPoliticoId == partidoId && c.Activo))
            return "No se puede desactivar este partido político porque tiene candidatos activos registrados.";

        if (await _context.DirigentesPoliticos.Include(d => d.Usuario)
                .AnyAsync(d => d.PartidoPoliticoId == partidoId && d.Usuario.Activo))
            return "No se puede desactivar este partido político porque tiene un dirigente político asignado.";

        return null;
    }
}
