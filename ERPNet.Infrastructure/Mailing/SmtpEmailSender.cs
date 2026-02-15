using ERPNet.Application.Mailing;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ERPNet.Infrastructure.Mailing;

public class SmtpEmailSender(IOptions<EmailSettings> options) : IEmailSender
{
    private readonly EmailSettings _settings = options.Value;

    public async Task EnviarAsync(string para, string asunto, string html, CancellationToken ct = default)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.RemitenteNombre, _settings.RemitenteEmail));
        email.To.Add(MailboxAddress.Parse(para));
        email.Subject = asunto;
        email.Body = new TextPart("html") { Text = html };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, ct);

        if (!string.IsNullOrEmpty(_settings.Usuario))
            await smtp.AuthenticateAsync(_settings.Usuario, _settings.Password, ct);

        await smtp.SendAsync(email, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}
