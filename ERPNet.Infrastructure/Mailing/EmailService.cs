using ERPNet.Application.Mailing;

namespace ERPNet.Infrastructure.Mailing;

public class EmailService(EmailChannel channel) : IEmailService
{
    public async Task EnviarAsync(MensajeEmail mensaje) =>
        await channel.EscribirAsync(mensaje);
}
