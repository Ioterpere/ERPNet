using System.Text.Json;

namespace ERPNet.Application.Messaging.Mensajes;

public record EmailMensaje(List<string> Destinatarios, string Asunto, string Plantilla, JsonElement Modelo);
