using ERPNet.Application.Mailing.Models;

namespace ERPNet.Application.Mailing;

public interface IMailService
{
    Task EnviarAsync(string para, string asunto, string cuerpo, CancellationToken ct = default);
    Task EnviarAsync(List<string> destinatarios, string asunto, string cuerpo, CancellationToken ct = default);
    Task EnviarBienvenidaAsync(string para, string nombre, string contrasenaTemp, CancellationToken ct = default);
    Task EnviarExcepcionAsync(List<string> destinatarios, ExcepcionEmailModel modelo, CancellationToken ct = default);
}
