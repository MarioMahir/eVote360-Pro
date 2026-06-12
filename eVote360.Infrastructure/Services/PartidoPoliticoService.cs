using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Services;

public class PartidoPoliticoService : IPartidoPoliticoService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;

    public PartidoPoliticoService(
        AppDbContext context,
        IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<List<PartidoPolitico>> GetAllAsync()
    {
        return await _context.PartidosPoliticos.ToListAsync();
    }

    public async Task<PartidoPolitico?> GetByIdAsync(int id)
    {
        return await _context.PartidosPoliticos.FindAsync(id);
    }

    public async Task<(bool Success, string Error)> CreateAsync(
        PartidoPolitico partido,
        IFormFile logo)
    {
        partido.Siglas =
            partido.Siglas.Trim().ToUpper();

        if (await _context.PartidosPoliticos
            .AnyAsync(x => x.Siglas == partido.Siglas))
        {
            return (
                false,
                "Ya existe un partido político registrado con estas siglas.");
        }

        if (logo == null)
        {
            return (
                false,
                "Debe seleccionar un logo.");
        }

        if (!_imageService.IsValidImage(logo))
        {
            return (
                false,
                "El logo del partido debe ser una imagen válida.");
        }

        partido.LogoUrl =
            await _imageService.SavePartyLogoAsync(
                logo,
                partido.Siglas);

        _context.PartidosPoliticos.Add(partido);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(
        PartidoPolitico partido,
        IFormFile? logo)
    {
        partido.Siglas =
            partido.Siglas.Trim().ToUpper();

        bool siglasDuplicadas =
            await _context.PartidosPoliticos
            .AnyAsync(x =>
                x.Id != partido.Id &&
                x.Siglas == partido.Siglas);

        if (siglasDuplicadas)
        {
            return (
                false,
                "Ya existe un partido político registrado con estas siglas.");
        }

        var partidoDb =
            await _context.PartidosPoliticos
            .FirstOrDefaultAsync(x => x.Id == partido.Id);

        if (partidoDb == null)
        {
            return (
                false,
                "Partido político no encontrado.");
        }

        partidoDb.Nombre = partido.Nombre;
        partidoDb.Descripcion = partido.Descripcion;
        partidoDb.Siglas = partido.Siglas;
        partidoDb.Activo = partido.Activo;

        if (logo != null)
        {
            if (!_imageService.IsValidImage(logo))
            {
                return (
                    false,
                    "El logo del partido debe ser una imagen válida.");
            }

            _imageService.DeleteImage(
                partidoDb.LogoUrl);

            partidoDb.LogoUrl =
                await _imageService.SavePartyLogoAsync(
                    logo,
                    partido.Siglas);
        }

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id)
    {
        var partido =
            await _context.PartidosPoliticos.FindAsync(id);

        if (partido == null)
        {
            return (
                false,
                "Partido político no encontrado.");
        }

        if (partido.Activo)
        {
            return (
                false,
                "Este partido político ya se encuentra activo.");
        }

        partido.Activo = true;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id)
    {
        var partido =
            await _context.PartidosPoliticos.FindAsync(id);

        if (partido == null)
        {
            return (
                false,
                "Partido político no encontrado.");
        }

        if (!partido.Activo)
        {
            return (
                false,
                "Este partido político ya se encuentra inactivo.");
        }

        partido.Activo = false;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}