using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Services;

namespace ERPNet.Web.Blazor.Client.Components.Common;

public partial class ChatPanel
{
    // ── Modelo de mensajes UI ────────────────────────────────────────────
    private abstract class Mensaje { }
    private sealed class MensajeUsuario(string texto, List<AdjuntoLocal> adjuntos) : Mensaje
    {
        public string Texto => texto;
        public List<AdjuntoLocal> Adjuntos => adjuntos;
    }
    private sealed class MensajeAsistente(string texto) : Mensaje
    {
        public string Texto => texto;
    }
    private sealed class MensajeSeleccion(List<ItemSeleccionable> opciones) : Mensaje
    {
        public List<ItemSeleccionable> Opciones => opciones;
    }

    // ── Adjuntos locales ─────────────────────────────────────────────────
    private sealed class AdjuntoLocal(string nombre, string contentType, string datosBase64)
    {
        public string Nombre => nombre;
        public string ContentType => contentType;
        public string DatosBase64 => datosBase64;
        public bool EsImagen => contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        public string DataUrl => $"data:{contentType};base64,{datosBase64}";
    }

    // ── Deserialización SSE ──────────────────────────────────────────────
    private record EventoStream(
        [property: JsonPropertyName("tipo")]      string     Tipo,
        [property: JsonPropertyName("contenido")] string?    Contenido,
        [property: JsonPropertyName("texto")]     string?    Texto,
        [property: JsonPropertyName("accion")]    AccionUi?  Accion);

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Estado ───────────────────────────────────────────────────────────
    private readonly List<Mensaje> _mensajes = [];
    private readonly List<AdjuntoLocal> _adjuntos = [];
    private string? _sessionId;
    private string _inputText = "";
    private bool _loading;
    private string? _toolActivo;
    private string? _error;
    private bool _shouldScroll;
    private bool _shouldFocusInput;
    private ElementReference _textareaRef;
    private InputFile? _inputFileRef;
    private bool _escuchando;
    private DotNetObjectReference<ChatPanel>? _selfRef;
    private int _seleccionFoco;
    private ElementReference _seleccionRef;

    // ── Ciclo de vida ────────────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive) return;
        _selfRef = DotNetObjectReference.Create(this);
        await IniciarSesionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try { await JS.InvokeVoidAsync("stt.stop"); } catch { }
        _selfRef?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldScroll)
        {
            _shouldScroll = false;
            try { await JS.InvokeVoidAsync("chat.scrollToBottom", "chat-messages"); } catch { }
        }
        if (_shouldFocusInput)
        {
            _shouldFocusInput = false;
            var seleccion = _mensajes.OfType<MensajeSeleccion>().LastOrDefault();
            if (seleccion is not null)
                try { await _seleccionRef.FocusAsync(); } catch { }
            else
                try { await _textareaRef.FocusAsync(); } catch { }
        }
    }

    private async Task IniciarSesionAsync()
    {
        try
        {
            var resp = await AiClient.SesionesAsync();
            _sessionId = resp.SessionId;
        }
        catch { /* se reintentará en el primer mensaje */ }
    }

    // ── Adjuntos ─────────────────────────────────────────────────────────
    private async Task AbrirFilePicker()
        => await JS.InvokeVoidAsync("chat.clickFileInput", "chat-file-input");

    private async Task OnFilesSelected(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles(5))
        {
            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                var buffer = new byte[file.Size];
                await stream.ReadExactlyAsync(buffer);
                _adjuntos.Add(new AdjuntoLocal(file.Name, file.ContentType, Convert.ToBase64String(buffer)));
            }
            catch { }
        }
    }

    // ── Envío ────────────────────────────────────────────────────────────
    private async Task SendMessage()
    {
        var texto = _inputText.Trim();
        if ((string.IsNullOrEmpty(texto) && _adjuntos.Count == 0) || _loading) return;

        _inputText = "";
        _error = null;

        var adjuntosEnvio = _adjuntos.ToList();
        _adjuntos.Clear();

        _mensajes.Add(new MensajeUsuario(texto, adjuntosEnvio));
        await EnviarAsync(texto, adjuntosEnvio);
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
            await SendMessage();
    }

    // ── Chat ─────────────────────────────────────────────────────────────
    private async Task EnviarAsync(string texto, List<AdjuntoLocal> adjuntosEnvio)
    {
        if (_sessionId is null)
            await IniciarSesionAsync();

        if (_sessionId is null)
        {
            _error = "No se pudo iniciar la sesión con el asistente.";
            return;
        }

        _loading = true;
        _shouldScroll = true;
        StateHasChanged();

        try
        {
            var payload = new NuevoMensajeRequest
            {
                Mensaje      = texto,
                PaginaActual = new Uri(Nav.Uri).LocalPath,
                Adjuntos     = adjuntosEnvio.Count > 0
                    ? adjuntosEnvio.Select(a => new ArchivoAdjunto { Nombre = a.Nombre, ContentType = a.ContentType, DatosBase64 = a.DatosBase64 }).ToList()
                    : null
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, $"api/ai/sesiones/{_sessionId}/mensajes");
            req.Content = JsonContent.Create(payload, options: _jsonOpts);

            using var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync() is { } line)
            {
                if (!line.StartsWith("data: ")) continue;

                var evento = JsonSerializer.Deserialize<EventoStream>(line["data: ".Length..], _jsonOpts);
                if (evento is null) continue;

                if (evento.Tipo == "Herramienta")
                {
                    _toolActivo = evento.Contenido;
                    await InvokeAsync(StateHasChanged);
                    continue;
                }

                if (evento.Tipo == "Fin")
                {
                    _mensajes.Add(new MensajeAsistente(evento.Texto ?? ""));

                    if (evento.Accion is { } accion)
                    {
                        if (accion.Tipo == TipoAccionUi.RellenarFormulario && accion.Ruta is not null)
                        {
                            Prefillado.Guardar(accion);
                            await JS.InvokeVoidAsync("chat.hide");
                            Nav.NavigateTo(accion.Ruta);
                            return;
                        }
                        if (accion.Tipo == TipoAccionUi.AbrirRegistro &&
                            accion.Ruta is not null &&
                            accion.Datos is JsonElement jeId &&
                            jeId.TryGetInt32(out var id))
                        {
                            await JS.InvokeVoidAsync("chat.hide");
                            Nav.NavigateTo($"{accion.Ruta}?id={id}");
                            return;
                        }
                        if (accion.Tipo == TipoAccionUi.ElegirOpcion &&
                            accion.Opciones is { Count: > 0 } opciones)
                        {
                            _seleccionFoco = 0;
                            _mensajes.Add(new MensajeSeleccion(opciones.ToList()));
                        }
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _error = $"Error: {ex.Message}";
        }
        finally
        {
            _toolActivo = null;
            _loading = false;
            _shouldScroll = true;
            _shouldFocusInput = true;
            StateHasChanged();
        }
    }

    // ── Selección de item ─────────────────────────────────────────────────
    private void OnSeleccionKeyDown(KeyboardEventArgs e, MensajeSeleccion sel)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                _seleccionFoco = (_seleccionFoco + 1) % sel.Opciones.Count;
                break;
            case "ArrowUp":
                _seleccionFoco = (_seleccionFoco - 1 + sel.Opciones.Count) % sel.Opciones.Count;
                break;
            case "Enter":
                _ = ConfirmarSeleccion(sel, sel.Opciones[_seleccionFoco]);
                break;
        }
    }

    private async Task ConfirmarSeleccion(MensajeSeleccion sel, ItemSeleccionable item)
    {
        _mensajes.Remove(sel);
        var texto = $"Seleccionado: {item.Etiqueta} (ID: {item.Id})";
        _mensajes.Add(new MensajeUsuario(texto, []));
        await EnviarAsync(texto, []);
    }

    // ── Speech to text ───────────────────────────────────────────────────
    private async Task ToggleMic()
    {
        if (_escuchando)
        {
            await JS.InvokeVoidAsync("stt.stop");
            _escuchando = false;
            return;
        }
        var ok = await JS.InvokeAsync<bool>("stt.start", _selfRef);
        if (ok) _escuchando = true;
    }

    [JSInvokable]
    public void OnSpeechResult(string texto)
    {
        _inputText = texto;
        _escuchando = false;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnSpeechEnd()
    {
        _escuchando = false;
        StateHasChanged();
    }
}
