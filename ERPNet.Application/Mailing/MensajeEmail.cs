namespace ERPNet.Application.Mailing;

public record MensajeEmail(
    string Para,
    string Asunto,
    string Plantilla,
    object Modelo);
