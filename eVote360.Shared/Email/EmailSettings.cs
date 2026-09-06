namespace eVote360.Shared.Email;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    /// <summary>Servidor SMTP. Si está vacío, los correos se guardan como archivos HTML en <see cref="CarpetaSalida"/>.</summary>
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromName { get; set; } = "eVote360";

    public string FromEmail { get; set; } = "no-reply@evote360.local";

    /// <summary>Carpeta donde se escriben los correos cuando no hay SMTP configurado (relativa a la raíz de la app).</summary>
    public string CarpetaSalida { get; set; } = "App_Data/correos";

    public bool SmtpConfigurado => !string.IsNullOrWhiteSpace(Host);
}
