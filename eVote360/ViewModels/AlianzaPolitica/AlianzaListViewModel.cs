namespace eVote360.ViewModels.AlianzaPolitica;

public class AlianzaListViewModel
{
    public int Id { get; set; }

    public string PartidoSolicitante { get; set; } = string.Empty;

    public string PartidoAliado { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaSolicitud { get; set; }
}