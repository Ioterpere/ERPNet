using ERPNet.Application.Email;
using ERPNet.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ERPNet.Infrastructure.Email;

public class EmailService(
    IServiceScopeFactory scopeFactory,
    IOptions<EmailSettings> options) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public Task EnviarAsync(MensajeEmail mensaje)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var renderer = scope.ServiceProvider.GetRequiredService<RazorViewToStringRenderer>();

                var html = await renderer.RenderAsync(mensaje.Plantilla, mensaje.Modelo);

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_settings.RemitenteNombre, _settings.RemitenteEmail));
                email.To.Add(MailboxAddress.Parse(mensaje.Para));
                email.Subject = mensaje.Asunto;
                email.Body = new TextPart("html") { Text = html };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl);

                if (!string.IsNullOrEmpty(_settings.Usuario))
                    await smtp.AuthenticateAsync(_settings.Usuario, _settings.Password);

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                try
                {
                    using var logScope = scopeFactory.CreateScope();
                    var logService = logScope.ServiceProvider.GetRequiredService<ILogService>();
                    await logService.WarningAsync($"ErrorEmail {mensaje.Para} ({mensaje.Asunto}): {ex.Message}");
                }
                catch
                {
                    // Si falla el log, no hay más que hacer
                }
            }
        });

        return Task.CompletedTask;
    }
}
