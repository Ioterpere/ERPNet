using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ERPNet.Web.Blazor.Client.Mcp;

/// <summary>
/// Servicio para registrar y desregistrar tools desde componentes Blazor.
///
/// Actúa como puente en dos sentidos:
/// 1. Hacia afuera (WebMCP): expone los tools a agentes externos via navigator.modelContext.
/// 2. Hacia adentro (ChatPanel): el chat usa ExecuteTool() y GetOpenAiToolsJson()
///    para ejecutar los tools que estén activos en la página actual.
///
/// IMPORTANTE: los handlers y metadata se almacenan SIEMPRE, independientemente
/// de si WebMCP está disponible en el browser. Así el chat funciona en cualquier browser.
/// </summary>
public sealed class McpToolService : IAsyncDisposable
{
    private readonly IJSRuntime _js;

    // Handlers C# — se ejecutan tanto por agentes externos (via JS interop) como por el chat interno.
    private readonly Dictionary<string, Func<JsonElement, Task<string>>> _handlers = [];

    // Metadata de cada tool (description + schema) para construir el payload de OpenAI.
    private readonly Dictionary<string, ToolMeta> _toolMeta = [];

    private DotNetObjectReference<McpToolService>? _dotnetRef;
    private bool _initialized;
    private readonly HashSet<string> _pageTools = [];
    private bool _shouldCloseChat;

    /// <summary>True si navigator.modelContext está disponible (polyfill cargado).</summary>
    public bool IsAvailable { get; private set; }

    private sealed record ToolMeta(string Description, object InputSchema);

    public McpToolService(IJSRuntime js) => _js = js;

    // ── Inicialización ──────────────────────────────────────────────────

    /// <summary>
    /// Inicializa el servicio. Registra los tools globales tanto en C# como en WebMCP.
    /// Llamar una vez desde MainLayout.OnAfterRenderAsync.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        // Tools globales: siempre disponibles para el chat interno.
        RegisterGlobalHandlers();

        // WebMCP: solo si el polyfill está disponible (para agentes externos).
        try
        {
            IsAvailable = await _js.InvokeAsync<bool>("erpnetMcp.isAvailable");
            if (!IsAvailable) return;

            _dotnetRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("erpnetMcp.registerGlobalTools");
        }
        catch { IsAvailable = false; }
    }

    private void RegisterGlobalHandlers()
    {
        _handlers["erpnet_info"] = _ => Task.FromResult(JsonSerializer.Serialize(new
        {
            sistema = "ERPNet",
            version = "1.0.0",
            modulos = new[] { "RRHH", "Maquinaria", "Usuarios", "Roles" }
        }));
        _toolMeta["erpnet_info"] = new ToolMeta(
            "Devuelve información general del sistema ERPNet: nombre, versión y módulos.",
            new { type = "object", properties = new { } });
    }

    // ── Registro de tools por página ────────────────────────────────────

    /// <summary>
    /// Registra un tool de página. El handler recibe el input del agente y devuelve JSON con el resultado.
    /// Siempre almacena el handler y la metadata; solo registra en WebMCP si está disponible.
    /// </summary>
    public async Task RegisterToolAsync(
        string name,
        string description,
        object inputSchema,
        bool readOnly,
        Func<JsonElement, Task<string>> handler)
    {
        _pageTools.Add(name);
        _handlers[name] = handler;
        _toolMeta[name] = new ToolMeta(description, inputSchema);

        if (!IsAvailable) return;

        await _js.InvokeVoidAsync("erpnetMcp.registerTool", new
        {
            name,
            description,
            inputSchema,
            readOnly,
            dotnetRef = _dotnetRef
        });
    }

    /// <summary>
    /// Desregistra un tool. Llamar en DisposeAsync del componente que lo registró.
    /// </summary>
    public async Task UnregisterToolAsync(string name)
    {
        _pageTools.Remove(name);
        _handlers.Remove(name);
        _toolMeta.Remove(name);

        if (!IsAvailable) return;
        try
        {
            await _js.InvokeVoidAsync("erpnetMcp.unregisterTool", name);
        }
        catch (JSDisconnectedException) { /* circuito cerrado, limpieza JS innecesaria */ }
        catch (TaskCanceledException) { }
    }

    /// <summary>Desregistra todos los tools de página. Llamar en DisposeAsync del componente de página.</summary>
    public async Task UnregisterPageToolsAsync()
    {
        foreach (var name in _pageTools.ToList())
            await UnregisterToolAsync(name);
    }

    // ── API para el chat interno ─────────────────────────────────────────

    /// <summary>
    /// Devuelve la lista de tools actualmente registrados en formato JSON de OpenAI.
    /// El ChatPanel la usa para incluir los tools de la página activa en cada request.
    /// </summary>
    public string GetOpenAiToolsJson()
    {
        // Excluir erpnet_info del chat (no aporta valor en conversaciones)
        var tools = _toolMeta
            .Where(kvp => kvp.Key != "erpnet_info")
            .Select(kvp => new
            {
                type = "function",
                function = new
                {
                    name = kvp.Key,
                    description = kvp.Value.Description,
                    parameters = kvp.Value.InputSchema
                }
            });

        return JsonSerializer.Serialize(tools);
    }

    /// <summary>True si hay al menos un tool de página registrado (además de los globales).</summary>
    public bool HasPageTools => _toolMeta.Keys.Any(k => k != "erpnet_info");

    /// <summary>Señala al ChatPanel que debe cerrar el panel al terminar el turno actual.</summary>
    public void RequestCloseChat() => _shouldCloseChat = true;

    /// <summary>Consume la señal de cierre: devuelve true una sola vez y la resetea.</summary>
    public bool TakeCloseChat()
    {
        if (!_shouldCloseChat) return false;
        _shouldCloseChat = false;
        return true;
    }

    // ── Callbacks desde JS (agentes externos vía WebMCP) ────────────────

    /// <summary>Confirmación humana para operaciones de escritura. Siempre aprueba por ahora.</summary>
    [JSInvokable]
    public Task<bool> RequestConfirmation(string toolName, JsonElement input)
        => Task.FromResult(true);

    /// <summary>Ejecuta el handler C# del tool. Llamado tanto desde JS como desde ChatPanel.</summary>
    [JSInvokable]
    public async Task<string> ExecuteTool(string toolName, JsonElement input)
    {
        if (!_handlers.TryGetValue(toolName, out var handler))
            return JsonSerializer.Serialize(new { error = $"Tool '{toolName}' no registrado." });

        try
        {
            return await handler(input);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    public async ValueTask DisposeAsync()
    {
        // UnregisterToolAsync ya captura JSDisconnectedException internamente
        foreach (var name in _handlers.Keys.ToList())
            await UnregisterToolAsync(name);

        _dotnetRef?.Dispose();
    }
}
