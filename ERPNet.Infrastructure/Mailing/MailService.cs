using System.Text.Json;
using ERPNet.Application;
using ERPNet.Application.Mailing;
using ERPNet.Application.Mailing.Models;
using ERPNet.Application.Messaging;
using ERPNet.Application.Messaging.Mensajes;
using Microsoft.Extensions.Options;

namespace ERPNet.Infrastructure.Mailing;

public class MailService(
    IMessagePublisher<EmailMensaje> publisher,
    IOptions<ErpNetSettings> erpNetSettings) : IMailService
{
    private readonly string _erpWebClient = erpNetSettings.Value.ErpWebClient;
    public Task EnviarAsync(string para, string asunto, string cuerpo, CancellationToken ct = default) =>
        EnviarAsync([para], asunto, cuerpo, ct);

    public async Task EnviarAsync(List<string> destinatarios, string asunto, string cuerpo, CancellationToken ct = default)
    {
        await publisher.PublicarAsync(
            new EmailMensaje(
                destinatarios,
                asunto,
                PlantillaEmail.Texto,
                JsonSerializer.SerializeToElement(new TextoEmailModel(cuerpo))),
            ct);
    }

    public async Task EnviarBienvenidaAsync(string para, string nombre, string contrasenaTemp, CancellationToken ct = default)
    {
        var modelo = new BienvenidaEmailModel(nombre, _erpWebClient, contrasenaTemp);

        await publisher.PublicarAsync(
            new EmailMensaje(
                [para],
                "Bienvenido a ERPNet",
                PlantillaEmail.Bienvenida,
                JsonSerializer.SerializeToElement(modelo)),
            ct);
    }

    public async Task EnviarExcepcionAsync(List<string> destinatarios, ExcepcionEmailModel modelo, CancellationToken ct = default)
    {
        var asunto = $"[ERPNet] Excepcion: {modelo.Mensaje[..Math.Min(50, modelo.Mensaje.Length)]}";

        await publisher.PublicarAsync(
            new EmailMensaje(
                destinatarios,
                asunto,
                PlantillaEmail.Excepcion,
                JsonSerializer.SerializeToElement(modelo)),
            ct);
    }
}
