namespace eVote360.ViewModels;

/// <summary>Pantalla genérica de confirmación con botones Cancelar y Aceptar.</summary>
public class ConfirmacionViewModel
{
    public string Titulo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public string? Detalle { get; set; }

    public string Accion { get; set; } = string.Empty;

    public string Controlador { get; set; } = string.Empty;

    public int Id { get; set; }

    public string TextoAceptar { get; set; } = "Aceptar";

    public string ClaseBoton { get; set; } = "btn-primary";

    public string AccionCancelar { get; set; } = "Index";
}
