using System.Text.RegularExpressions;
using eVote360.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace eVote360.Infrastructure.Services;

/// <summary>Lectura de la cédula con el motor Tesseract (modelo en la carpeta tessdata).</summary>
public class OcrService : IOcrService
{
    private readonly string _tessdataPath;
    private readonly ILogger<OcrService> _logger;

    public OcrService(string tessdataPath, ILogger<OcrService> logger)
    {
        _tessdataPath = tessdataPath;
        _logger = logger;
    }

    public async Task<(bool Success, string Error, string Texto)> ExtraerTextoAsync(Stream imagen)
    {
        try
        {
            using var memoria = new MemoryStream();
            await imagen.CopyToAsync(memoria);
            var bytes = memoria.ToArray();

            // Tesseract es síncrono y pesado: se ejecuta fuera del hilo de la petición.
            var texto = await Task.Run(() =>
            {
                using var motor = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
                using var pix = Pix.LoadFromMemory(bytes);
                using var pagina = motor.Process(pix);
                return pagina.GetText();
            });

            return (true, string.Empty, texto ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando la imagen con Tesseract");
            return (false, "No fue posible procesar la imagen cargada. Por favor, suba una imagen más clara.", string.Empty);
        }
    }

    public bool CoincideDocumento(string textoOcr, string numeroDocumento)
    {
        if (string.IsNullOrWhiteSpace(textoOcr) || string.IsNullOrWhiteSpace(numeroDocumento))
            return false;

        var documento = Regex.Replace(numeroDocumento, @"\D", "");

        // Une dígitos separados por guiones o espacios (001-1234567-8 => 00112345678)
        var soloDigitos = Regex.Replace(textoOcr, @"[^\d\n]", "");
        var lineas = soloDigitos.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        return lineas.Any(l => l.Contains(documento))
            || Regex.Replace(textoOcr, @"\D", "").Contains(documento);
    }
}
