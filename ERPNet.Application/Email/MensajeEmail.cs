namespace ERPNet.Application.Email;

public record MensajeEmail(
    string Para,
    string Asunto,
    string Plantilla,
    object Modelo);
