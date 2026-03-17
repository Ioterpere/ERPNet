using System.Collections.Concurrent;
using Microsoft.Extensions.AI;

namespace ERPNet.Infrastructure.Ai;

public interface IChatSessionStore
{
    string Crear(string usuarioId, string systemPrompt);
    List<ChatMessage>? ObtenerHistorial(string sessionId, string usuarioId);
}

internal sealed class InMemoryChatSessionStore : IChatSessionStore
{
    private readonly ConcurrentDictionary<string, ChatSession> _sessions = new();

    public string Crear(string usuarioId, string systemPrompt)
    {
        var id = Guid.NewGuid().ToString("N");
        var session = new ChatSession { UsuarioId = usuarioId };
        session.Historial.Add(new ChatMessage(ChatRole.System, systemPrompt));
        _sessions[id] = session;
        LimpiarSesionesAntiguas();
        return id;
    }

    public List<ChatMessage>? ObtenerHistorial(string sessionId, string usuarioId)
    {
        if (_sessions.TryGetValue(sessionId, out var session) && session.UsuarioId == usuarioId)
        {
            session.UltimaActividad = DateTime.UtcNow;
            return session.Historial;
        }
        return null;
    }

    private void LimpiarSesionesAntiguas()
    {
        var limite = DateTime.UtcNow.AddHours(-2);
        foreach (var (key, session) in _sessions)
            if (session.UltimaActividad < limite)
                _sessions.TryRemove(key, out _);
    }
}

internal sealed class ChatSession
{
    public required string UsuarioId { get; init; }
    public List<ChatMessage> Historial { get; } = [];
    public DateTime UltimaActividad { get; set; } = DateTime.UtcNow;
}
