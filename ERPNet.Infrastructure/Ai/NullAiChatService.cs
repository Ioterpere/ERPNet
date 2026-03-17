using System.Runtime.CompilerServices;
using ERPNet.Application.Ai;
using ERPNet.Application.Ai.DTOs;

namespace ERPNet.Infrastructure.Ai;

internal sealed class NullAiChatService : IAiChatService
{
    private const string Msg = "El módulo de IA no está habilitado.";

    public IReadOnlyList<string> ObtenerNombresHerramientas() => [];

    public Task<string> CrearSesionAsync(string usuarioId)
        => throw new InvalidOperationException(Msg);

    public async IAsyncEnumerable<ChatStreamEvent> ChatStreamAsync(
        string sessionId, string usuarioId, NuevoMensajeRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await Task.CompletedTask;
        yield return new ChatStreamEvent { Tipo = TipoChatStreamEvent.Fin, Texto = Msg };
    }
}
