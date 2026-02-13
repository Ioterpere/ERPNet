namespace ERPNet.Application.Mailing;

public interface IEmailService
{
    Task EnviarAsync(MensajeEmail mensaje);
}
