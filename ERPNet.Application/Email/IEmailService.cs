namespace ERPNet.Application.Email;

public interface IEmailService
{
    Task EnviarAsync(MensajeEmail mensaje);
}
