namespace eVote360.Shared.Email;

public interface IEmailService
{
    /// <summary>Envía un correo HTML. Devuelve false si el envío falla (no lanza excepción).</summary>
    Task<(bool Success, string Error)> EnviarAsync(string destinatario, string nombreDestinatario, string asunto, string cuerpoHtml);
}
