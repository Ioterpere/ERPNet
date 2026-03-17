using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using ERPNet.Application.Ai;
using ERPNet.Application.Ai.DTOs;
using ERPNet.Infrastructure.Ai.Tools.Common;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Enums;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ErpChat = ERPNet.Application.Ai.DTOs;

namespace ERPNet.Infrastructure.Ai;

public class AiChatService(
    IChatClient client,
    IEnumerable<McpToolBase> tools,
    IEnumerable<FormularioToolConfig> formularios,
    IAccionesUiCollector accionesUi,
    IChatSessionStore sessionStore,
    ICurrentUserProvider currentUser,
    IOptions<AiSettings> settings) : IAiChatService
{
    private readonly string _systemPrompt = settings.Value.SystemPrompt;

    public Task<string> CrearSesionAsync(string usuarioId)
        => Task.FromResult(sessionStore.Crear(usuarioId, _systemPrompt));

    public async IAsyncEnumerable<ChatStreamEvent> ChatStreamAsync(
        string sessionId, string usuarioId, ErpChat.NuevoMensajeRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var historial = sessionStore.ObtenerHistorial(sessionId, usuarioId);
        if (historial is null)
        {
            yield return new ChatStreamEvent { Tipo = TipoChatStreamEvent.Fin, Texto = "Sesión no encontrada o expirada." };
            yield break;
        }

        historial.Add(BuildUserMessage(request.Mensaje, request.Adjuntos, request.PaginaActual));

        var opciones = new ChatOptions { Tools = BuildTools(), ToolMode = ChatToolMode.Auto };
        var textoFinal = new StringBuilder();

        await foreach (var update in client.GetStreamingResponseAsync(historial, opciones, ct))
        {
            foreach (var content in update.Contents)
            {
                if (content is FunctionCallContent fc && !string.IsNullOrEmpty(fc.Name))
                    yield return new ChatStreamEvent { Tipo = TipoChatStreamEvent.Herramienta, Contenido = fc.Name };
                else if (content is TextContent tc)
                    textoFinal.Append(tc.Text);
            }
        }

        var texto = textoFinal.ToString();
        if (!string.IsNullOrEmpty(texto))
            historial.Add(new ChatMessage(ChatRole.Assistant, texto));

        yield return new ChatStreamEvent
        {
            Tipo    = TipoChatStreamEvent.Fin,
            Texto   = texto,
            Accion  = accionesUi.Obtener()
        };
    }

    private static ChatMessage BuildUserMessage(string mensaje, IReadOnlyList<ErpChat.ArchivoAdjunto>? adjuntos, string? paginaActual)
    {
        var texto = paginaActual is not null
            ? $"[Página actual: {paginaActual}]\n{mensaje}"
            : mensaje;

        if (adjuntos?.Count > 0)
        {
            var content = new List<AIContent>();
            if (!string.IsNullOrEmpty(texto))
                content.Add(new TextContent(texto));
            foreach (var adj in adjuntos)
                content.Add(new DataContent($"data:{adj.ContentType};base64,{adj.DatosBase64}"));

            return new ChatMessage(ChatRole.User, content);
        }
        return new ChatMessage(ChatRole.User, texto);
    }

    public IReadOnlyList<string> ObtenerNombresHerramientas()
    {
        var fromTools = tools
            .Where(t => TieneAcceso(t.Recurso))
            .SelectMany(t => t.GetAiFunctions())
            .OfType<AIFunction>()
            .Select(f => f.Name);

        var fromFormularios = formularios
            .Where(f => TieneAcceso(f.Recurso))
            .Select(f => f.Nombre);

        return [.. fromTools, .. fromFormularios];
    }

    private bool TieneAcceso(RecursoCodigo recurso)
    {
        var permisos = currentUser.Current?.Permisos;
        return permisos is not null && permisos.Any(p => p.Codigo == recurso);
    }

    private IList<AITool> BuildTools()
    {
        var fromTools = tools
            .Where(t => TieneAcceso(t.Recurso))
            .SelectMany(t => t.GetAiFunctions());

        var fromFormularios = formularios
            .Where(f => TieneAcceso(f.Recurso))
            .Select(f =>
            AIFunctionFactory.Create(f.ConstruirInvocable(accionesUi), new AIFunctionFactoryOptions
            {
                Name = f.Nombre,
                Description = f.Descripcion,
                JsonSchemaCreateOptions = new AIJsonSchemaCreateOptions
                {
                    TransformSchemaNode = (ctx, node) =>
                    {
                        if (ctx.PropertyInfo?.Name is { } nombre &&
                            f.Descripciones.TryGetValue(nombre, out var desc) &&
                            node is JsonObject obj)
                        {
                            obj["description"] = desc;
                        }
                        return node;
                    }
                }
            }));

        return [.. fromTools, .. fromFormularios];
    }
}
