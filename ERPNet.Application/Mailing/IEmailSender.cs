namespace ERPNet.Application.Mailing;

public interface IEmailSender
{
    Task EnviarAsync(string para, string asunto, string html, CancellationToken ct = default);
}
