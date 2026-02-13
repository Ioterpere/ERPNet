namespace ERPNet.Application.Mailing.Models;

public record ExcepcionEmailModel(
    string Mensaje,
    string Codigo,
    string? StackTrace,
    string? Origen,
    string? InnerExceptionMensaje,
    string? InnerExceptionStackTrace,
    string MetodoHttp,
    string Ruta,
    string? QueryString,
    string? UsuarioEmail,
    string? DireccionIp,
    DateTime Fecha);
