using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace eVote360.Shared.Email;

/// <summary>
/// Servicio de correo compartido. Con SMTP configurado envía por MailKit;
/// sin configuración, escribe cada correo como archivo HTML para poder
/// probar el flujo del elector en desarrollo.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly string _rutaBase;

    public SmtpEmailService(
        IOptions<EmailSettings> settings,
        ILogger<SmtpEmailService> logger,
        string rutaBase)
    {
        _settings = settings.Value;
        _logger = logger;
        _rutaBase = rutaBase;
    }

    public async Task<(bool Success, string Error)> EnviarAsync(
        string destinatario,
        string nombreDestinatario,
        string asunto,
        string cuerpoHtml)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
            return (false, "El destinatario no tiene correo electrónico.");

        if (!_settings.SmtpConfigurado)
            return await GuardarEnArchivoAsync(destinatario, nombreDestinatario, asunto, cuerpoHtml);

        try
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            mensaje.To.Add(new MailboxAddress(nombreDestinatario, destinatario));
            mensaje.Subject = asunto;
            mensaje.Body = new BodyBuilder { HtmlBody = cuerpoHtml }.ToMessageBody();

            using var cliente = new SmtpClient();

            var opcion = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await cliente.ConnectAsync(_settings.Host, _settings.Port, opcion);

            if (!string.IsNullOrWhiteSpace(_settings.User))
                await cliente.AuthenticateAsync(_settings.User, _settings.Password);

            await cliente.SendAsync(mensaje);
            await cliente.DisconnectAsync(true);

            _logger.LogInformation("Correo enviado a {Destinatario}: {Asunto}", destinatario, asunto);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo a {Destinatario}", destinatario);
            return (false, ex.Message);
        }
    }

    private async Task<(bool Success, string Error)> GuardarEnArchivoAsync(
        string destinatario,
        string nombreDestinatario,
        string asunto,
        string cuerpoHtml)
    {
        try
        {
            var carpeta = Path.Combine(_rutaBase, _settings.CarpetaSalida);
            Directory.CreateDirectory(carpeta);

            var nombre = $"{DateTime.Now:yyyyMMdd-HHmmss-fff}_{Sanear(destinatario)}.html";
            var ruta = Path.Combine(carpeta, nombre);

            var html = $"""
                <!DOCTYPE html>
                <html lang="es"><head><meta charset="utf-8"><title>{asunto}</title></head>
                <body style="font-family:Segoe UI,Arial,sans-serif;max-width:640px;margin:24px auto;padding:0 16px">
                <div style="background:#f1f3f5;border-radius:8px;padding:12px 16px;font-size:13px;color:#495057">
                  <strong>Correo simulado (sin SMTP configurado)</strong><br>
                  Para: {nombreDestinatario} &lt;{destinatario}&gt;<br>
                  Asunto: {asunto}<br>
                  Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                </div>
                <hr>
                {cuerpoHtml}
                </body></html>
                """;

            await File.WriteAllTextAsync(ruta, html);

            _logger.LogWarning(
                "SMTP no configurado. Correo para {Destinatario} guardado en {Ruta}",
                destinatario, ruta);

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo guardar el correo simulado");
            return (false, ex.Message);
        }
    }

    private static string Sanear(string texto)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            texto = texto.Replace(c, '_');

        return texto;
    }
}
