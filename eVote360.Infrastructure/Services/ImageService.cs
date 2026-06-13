using eVote360.Core.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace eVote360.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool IsValidImage(IFormFile file)
    {
        string extension =
            Path.GetExtension(file.FileName)
            .ToLower();

        string[] permitidas =
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        return permitidas.Contains(extension);
    }

    public async Task<string> SavePartyLogoAsync(
        IFormFile file,
        string siglas)
    {
        string extension =
            Path.GetExtension(file.FileName)
            .ToLower();

        string nombreArchivo =
            $"{siglas}_{Guid.NewGuid()}{extension}";

        string ruta =
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "partidos",
                nombreArchivo);

        using var stream =
            new FileStream(ruta, FileMode.Create);

        await file.CopyToAsync(stream);

        return "/uploads/partidos/" + nombreArchivo;
    }

    public void DeleteImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        string rutaFisica =
            Path.Combine(
                _environment.WebRootPath,
                imageUrl.TrimStart('/'));

        if (File.Exists(rutaFisica))
        {
            File.Delete(rutaFisica);
        }
    }

    public async Task<string> SaveCandidatePhotoAsync(
    IFormFile file,
    string nombre)
    {
        string extension =
            Path.GetExtension(file.FileName)
            .ToLower();

        string nombreArchivo =
            $"{nombre}_{Guid.NewGuid()}{extension}";

        string ruta =
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "candidatos",
                nombreArchivo);

        Directory.CreateDirectory(
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "candidatos"));

        using var stream =
            new FileStream(ruta, FileMode.Create);

        await file.CopyToAsync(stream);

        return "/uploads/candidatos/" + nombreArchivo;
    }
}