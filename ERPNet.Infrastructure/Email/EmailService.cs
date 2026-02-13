using ERPNet.Application.Email;

namespace ERPNet.Infrastructure.Email;

public class EmailService(EmailChannel channel) : IEmailService
{
    public async Task EnviarAsync(MensajeEmail mensaje) =>
        await channel.EscribirAsync(mensaje);
}
