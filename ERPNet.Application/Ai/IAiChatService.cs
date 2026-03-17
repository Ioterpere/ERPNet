using ERPNet.Application.Ai.DTOs;

namespace ERPNet.Application.Ai;

public interface IAiChatService
{
    Task<string> CrearSesionAsync(string usuarioId);

    IAsyncEnumerable<ChatStreamEvent> ChatStreamAsync(
        string sessionId,
        string usuarioId,
        NuevoMensajeRequest request,
        CancellationToken ct = default);

    IReadOnlyList<string> ObtenerNombresHerramientas();
}
