using eVote360.Core.DTOs.Candidatos;
using eVote360.Core.Entities;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Services;

public class CandidatoService : ICandidatoService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;

    public CandidatoService(
        AppDbContext context,
        IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<List<Candidato>> GetAllAsync(
        int partidoId)
    {
        return await _context.Candidatos
            .Include(x => x.PuestoElectivo)
            .Where(x => x.PartidoPoliticoId == partidoId)
            .ToListAsync();
    }

    public async Task<Candidato?> GetByIdAsync(
        int id,
        int partidoId)
    {
        return await _context.Candidatos
            .Include(x => x.PuestoElectivo)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PartidoPoliticoId == partidoId);
    }

    public async Task<(bool Success, string Error)> CreateAsync(
        CandidatoCreateDto dto,
        int partidoId)
    {
        if (!_imageService.IsValidImage(dto.Foto))
        {
            return (
                false,
                "La foto del candidato debe ser una imagen válida.");
        }

        string fotoUrl =
            await _imageService.SaveCandidatePhotoAsync(
                dto.Foto,
                $"{dto.Nombre}_{dto.Apellido}");

        Candidato candidato = new()
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            FotoUrl = fotoUrl,
            Activo = dto.Activo,
            PartidoPoliticoId = partidoId
        };

        _context.Candidatos.Add(candidato);

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(
        CandidatoUpdateDto dto,
        int partidoId)
    {
        var candidato =
            await _context.Candidatos
            .FirstOrDefaultAsync(x =>
                x.Id == dto.Id &&
                x.PartidoPoliticoId == partidoId);

        if (candidato == null)
        {
            return (
                false,
                "No tiene permisos para modificar este candidato.");
        }

        candidato.Nombre =
            dto.Nombre.Trim();

        candidato.Apellido =
            dto.Apellido.Trim();

        candidato.Activo =
            dto.Activo;

        if (dto.Foto != null)
        {
            if (!_imageService.IsValidImage(dto.Foto))
            {
                return (
                    false,
                    "La foto del candidato debe ser una imagen válida.");
            }

            _imageService.DeleteImage(
                candidato.FotoUrl);

            candidato.FotoUrl =
                await _imageService.SaveCandidatePhotoAsync(
                    dto.Foto,
                    $"{dto.Nombre}_{dto.Apellido}");
        }

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(
        int id,
        int partidoId)
    {
        var candidato =
            await _context.Candidatos
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PartidoPoliticoId == partidoId);

        if (candidato == null)
        {
            return (
                false,
                "No tiene permisos para modificar este candidato.");
        }

        if (candidato.Activo)
        {
            return (
                false,
                "Este candidato ya se encuentra activo.");
        }

        candidato.Activo = true;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(
        int id,
        int partidoId)
    {
        var candidato =
            await _context.Candidatos
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PartidoPoliticoId == partidoId);

        if (candidato == null)
        {
            return (
                false,
                "No tiene permisos para modificar este candidato.");
        }

        if (!candidato.Activo)
        {
            return (
                false,
                "Este candidato ya se encuentra inactivo.");
        }

        candidato.Activo = false;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}