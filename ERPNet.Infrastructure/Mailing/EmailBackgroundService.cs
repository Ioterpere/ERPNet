using ERPNet.Application.Mailing;
using ERPNet.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ERPNet.Infrastructure.Mailing;

public class EmailBackgroundService(
    EmailChannel channel,
    IServiceScopeFactory scopeFactory,
    IOptions<EmailSettings> options,
    ILogger<EmailBackgroundService> logger) : BackgroundService
{
    private readonly EmailSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var mensaje in channel.Reader.ReadAllAsync(stoppingToken))
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
                await smtp.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, stoppingToken);

                if (!string.IsNullOrEmpty(_settings.Usuario))
                    await smtp.AuthenticateAsync(_settings.Usuario, _settings.Password, stoppingToken);

                await smtp.SendAsync(email, stoppingToken);
                await smtp.DisconnectAsync(true, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                try
                {
                    using var logScope = scopeFactory.CreateScope();
                    var logService = logScope.ServiceProvider.GetRequiredService<ILogService>();
                    await logService.WarningAsync($"ErrorEmail {mensaje.Para} ({mensaje.Asunto}): {ex.Message}");
                }
                catch (Exception logEx)
                {
                    logger.LogError(logEx, "No se pudo registrar el error de email");
                }
            }
        }
    }
}
