using eVote360.Core.DTOs.Asignaciones;
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
    private readonly IEleccionService _elecciones;

    public CandidatoService(AppDbContext context, IImageService imageService, IEleccionService elecciones)
    {
        _context = context;
        _imageService = imageService;
        _elecciones = elecciones;
    }

    public async Task<List<CandidatoListDto>> GetAllAsync(int partidoId)
    {
        var candidatos = await _context.Candidatos
            .Where(x => x.PartidoPoliticoId == partidoId)
            .OrderBy(x => x.Apellido).ThenBy(x => x.Nombre)
            .ToListAsync();

        // Puesto asociado dentro del partido de origen del candidato
        var puestos = await _context.AsignacionesCandidatoPuesto
            .Include(a => a.PuestoElectivo)
            .Where(a => a.PartidoPoliticoId == partidoId)
            .ToDictionaryAsync(a => a.CandidatoId, a => a.PuestoElectivo.Nombre);

        return candidatos.Select(c => new CandidatoListDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            FotoUrl = c.FotoUrl,
            Activo = c.Activo,
            PuestoAsociado = puestos.GetValueOrDefault(c.Id)
        }).ToList();
    }

    public Task<Candidato?> GetByIdAsync(int id, int partidoId) =>
        _context.Candidatos.FirstOrDefaultAsync(x => x.Id == id && x.PartidoPoliticoId == partidoId);

    public async Task<(bool Success, string Error)> CreateAsync(CandidatoCreateDto dto, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede crear un candidato mientras exista una elección activa.");

        var partido = await _context.PartidosPoliticos.FindAsync(partidoId);

        if (partido == null)
            return (false, "No puede crear candidatos porque no tiene un partido político asignado.");

        if (!partido.Activo)
            return (false, "No puede crear candidatos porque el partido político asignado se encuentra inactivo.");

        if (dto.Foto == null || dto.Foto.Length == 0)
            return (false, "La foto del candidato es requerida.");

        if (!_imageService.IsValidImage(dto.Foto))
            return (false, "La foto del candidato debe ser una imagen válida.");

        var fotoUrl = await _imageService.SaveCandidatePhotoAsync(dto.Foto, $"{dto.Nombre.Trim()}_{dto.Apellido.Trim()}");

        _context.Candidatos.Add(new Candidato
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            FotoUrl = fotoUrl,
            Activo = dto.Activo,
            PartidoPoliticoId = partidoId
        });

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(CandidatoUpdateDto dto, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede editar un candidato mientras exista una elección activa.");

        var candidato = await GetByIdAsync(dto.Id, partidoId);

        if (candidato == null)
            return (false, "No tiene permisos para modificar este candidato.");

        var nombre = dto.Nombre.Trim();
        var apellido = dto.Apellido.Trim();
        var cambiaDatos = candidato.Nombre != nombre || candidato.Apellido != apellido || dto.Foto != null;

        if (cambiaDatos && await _elecciones.CandidatoParticipoAsync(candidato.Id))
            return (false, "No se pueden modificar los datos principales de este candidato porque ya participó en una elección.");

        if (candidato.Activo && !dto.Activo && await EstaAsignadoAsync(candidato.Id))
            return (false, "No se puede desactivar este candidato porque está asignado a un puesto electivo.");

        if (dto.Foto != null)
        {
            if (!_imageService.IsValidImage(dto.Foto))
                return (false, "La foto del candidato debe ser una imagen válida.");

            _imageService.DeleteImage(candidato.FotoUrl);
            candidato.FotoUrl = await _imageService.SaveCandidatePhotoAsync(dto.Foto, $"{nombre}_{apellido}");
        }

        candidato.Nombre = nombre;
        candidato.Apellido = apellido;
        candidato.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivarAsync(int id, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede activar un candidato mientras exista una elección activa.");

        var candidato = await _context.Candidatos
            .Include(c => c.PartidoPolitico)
            .FirstOrDefaultAsync(x => x.Id == id && x.PartidoPoliticoId == partidoId);

        if (candidato == null)
            return (false, "No tiene permisos para activar este candidato.");

        if (candidato.Activo)
            return (false, "Este candidato ya se encuentra activo.");

        if (!candidato.PartidoPolitico.Activo)
            return (false, "No se puede activar este candidato porque su partido político se encuentra inactivo.");

        candidato.Activo = true;
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DesactivarAsync(int id, int partidoId)
    {
        if (await _elecciones.ExisteEleccionActivaAsync())
            return (false, "No se puede desactivar un candidato mientras exista una elección activa.");

        var candidato = await GetByIdAsync(id, partidoId);

        if (candidato == null)
            return (false, "No tiene permisos para desactivar este candidato.");

        if (!candidato.Activo)
            return (false, "Este candidato ya se encuentra inactivo.");

        if (await EstaAsignadoAsync(id))
            return (false, "No se puede desactivar este candidato porque está asignado a un puesto electivo.");

        candidato.Activo = false;
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    private Task<bool> EstaAsignadoAsync(int candidatoId) =>
        _context.AsignacionesCandidatoPuesto.AnyAsync(a => a.CandidatoId == candidatoId);
}
