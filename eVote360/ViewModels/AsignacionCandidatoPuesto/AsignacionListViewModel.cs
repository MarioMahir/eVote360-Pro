namespace eVote360.ViewModels.AsignacionCandidatoPuesto;

public class AsignacionListViewModel
{
    public int Id { get; set; }

    public string Eleccion { get; set; } = string.Empty;

    public string Candidato { get; set; } = string.Empty;

    public string PuestoElectivo { get; set; } = string.Empty;

    public DateTime FechaAsignacion { get; set; }

    public bool Activo { get; set; }
}