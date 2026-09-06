namespace eVote360.Core.Interfaces.Services;

public interface IOcrService
{
    /// <summary>Extrae el texto de una imagen usando Tesseract.</summary>
    Task<(bool Success, string Error, string Texto)> ExtraerTextoAsync(Stream imagen);

    /// <summary>Indica si el número de documento aparece en el texto extraído (ignorando guiones y espacios).</summary>
    bool CoincideDocumento(string textoOcr, string numeroDocumento);
}
