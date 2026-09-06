using System.Text.Json;

namespace eVote360.Helpers;

/// <summary>Estado del proceso de votación del elector, guardado en la sesión del servidor.</summary>
public class SesionElector
{
    private const string Clave = "Elector";

    public int CiudadanoId { get; set; }

    public int EleccionId { get; set; }

    public string NumeroDocumento { get; set; } = string.Empty;

    public bool OcrValidado { get; set; }

    public bool CodigoValidado { get; set; }

    /// <summary>Puesto electivo → candidato seleccionado (null = "Ninguno").</summary>
    public Dictionary<int, int?> Selecciones { get; set; } = [];

    public bool Iniciado => CiudadanoId > 0 && EleccionId > 0;

    public static SesionElector? Leer(ISession session)
    {
        var json = session.GetString(Clave);
        return json == null ? null : JsonSerializer.Deserialize<SesionElector>(json);
    }

    public void Guardar(ISession session) =>
        session.SetString(Clave, JsonSerializer.Serialize(this));

    public static void Limpiar(ISession session) => session.Remove(Clave);
}
