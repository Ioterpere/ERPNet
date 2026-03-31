using System.Text.Json;
using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Components.Common.Toast;
using ERPNet.Web.Blazor.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Common;

/// <summary>
/// Clase base para páginas del Erp.
/// Gestiona: parámetro Id, estado de paginación/búsqueda,
/// navegación entre registros, atajos de teclado e
/// inicialización del formulario de creación desde fuente externa.
/// </summary>
[Authorize]
public abstract class ErpPageBase : PageBase, IAsyncDisposable
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ToastService Toast { get; set; } = default!;
    [Inject] private PrefilladoService _prefillado { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "id")]
    public int? Id { get; set; }

    [SupplyParameterFromQuery(Name = "tab")]
    public string? Tab { get; set; }

    protected void CambiarTab(string tabId)
        => Nav.NavigateTo(Nav.GetUriWithQueryParameter("tab", tabId));

    // ── Layout ref ────────────────────────────────────────────
    protected ErpPage? _refLayout;

    // ── Estado compartido ──────────────────────────────────────
    protected bool _esNuevo;
    protected string _busqueda = string.Empty;
    private bool _enfocarBusqueda;

    private CancellationTokenSource? _ctsBusqueda;

    protected async Task OnBusquedaInputAsync(ChangeEventArgs e)
    {
        _busqueda = e.Value?.ToString() ?? string.Empty;
        if (_ctsBusqueda is not null) await _ctsBusqueda.CancelAsync();
        _ctsBusqueda = new CancellationTokenSource();
        try
        {
            await Task.Delay(300, _ctsBusqueda.Token);
            await CargarListaAsync();
        }
        catch (OperationCanceledException)
        {
            // debounce cancelado intencionalmente — no requiere acción
        }
    }

    // ── Modal de eliminación ───────────────────────────────────
    protected bool _mostrarModalEliminar;
    private bool _enfocarEliminar;

    protected void AbrirModalEliminar()
    {
        _mostrarModalEliminar = true;
        _enfocarEliminar      = true;
    }

    protected Task EnfocarBtnEliminarAsync() => _refLayout?.FocusBtnEliminarAsync() ?? Task.CompletedTask;

    // ── Foco en formulario de creación / detalle ───────────────
    protected bool _enfocarNuevo;
    private bool _enfocarDetalle;

    protected virtual Task EnfocarPrimerCampoNuevoAsync()  => Task.CompletedTask;
    protected virtual Task EnfocarPrimerCampoFiltroAsync() => Task.CompletedTask;

    // ── Abstracts: ciclo de vida de datos ─────────────────────
    protected abstract Task CargarListaAsync();
    protected abstract Task CargarDetalleAsync(int id);
    protected abstract Task LimpiarDetalleAsync();

    // ── Abstracts: acciones de teclado ────────────────────────
    protected abstract Task OnNuevo();
    protected abstract Task OnGuardar();
    protected abstract Task OnBorrar();
    protected bool _enfocarFiltro;

    protected virtual Task OnFiltro()        => Task.CompletedTask;
    protected virtual Task OnLimpiarFiltro() => Task.CompletedTask;
    protected virtual Task OnEscape()
    {
        if (_mostrarModalEliminar)
        {
            _mostrarModalEliminar = false;
            return Task.CompletedTask;
        }
        if (_esNuevo)
        {
            VolverALista();
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    // ── Inicialización de creación desde fuente externa ────────
    protected virtual Task InicializarCreacion(JsonElement datos) => Task.CompletedTask;

    private void OnPrefilladoHandler(string ruta)
    {
        if (ruta != new Uri(Nav.Uri).LocalPath) return;
        _ = InvokeAsync(async () =>
        {
            var accion = _prefillado.Consumir(ruta);
            if (accion is null) return;
            await AplicarCreacionAsync(accion);
            StateHasChanged();
        });
    }

    private async Task AplicarCreacionAsync(AccionUi accion)
    {
        if (accion.Datos is not JsonElement je) return;
        _esNuevo = true;
        _enfocarNuevo = true;
        await InicializarCreacion(je);
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
    }

    // ── Ciclo de vida ──────────────────────────────────────────
    private int? _idActual;
    private string? _tabActual;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (!RendererInfo.IsInteractive) return;

        _prefillado.OnPrefillado += OnPrefilladoHandler;

        var accion = _prefillado.Consumir(new Uri(Nav.Uri).LocalPath);
        if (accion is not null) await AplicarCreacionAsync(accion);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Id == _idActual)
        {
            // Solo cambió el tab con un ítem activo → enfocar primer campo de la nueva tab
            if (Tab != _tabActual && Id.HasValue)
            {
                _tabActual = Tab;
                _enfocarDetalle = true;
            }
            return;
        }
        _tabActual = Tab;
        _idActual = Id;

        if (Id.HasValue)
        {
            _esNuevo = false;
            await CargarDetalleAsync(Id.Value);
            _enfocarDetalle = true;
        }
        else
        {
            // _esNuevo NO se resetea aquí: OnNuevo() lo pone a true
            // antes de navegar, y esta rama solo limpia el detalle.
            await LimpiarDetalleAsync();
        }
    }

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ErpPageBase>? _dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/erp-shortcuts.js");
            await _jsModule.InvokeVoidAsync("registerShortcuts", _dotNetRef);
        }

        if (_enfocarNuevo)
        {
            _enfocarNuevo = false;
            await EnfocarPrimerCampoNuevoAsync();
        }

        if (_enfocarDetalle)
        {
            _enfocarDetalle = false;
            if (_jsModule is not null)
                await _jsModule.InvokeVoidAsync("enfocarPrimerCampoDetalle");
        }

        if (_enfocarFiltro)
        {
            _enfocarFiltro = false;
            await EnfocarPrimerCampoFiltroAsync();
        }

        if (_enfocarBusqueda)
        {
            _enfocarBusqueda = false;
            if (_refLayout is not null) await _refLayout.FocusSearchAsync();
        }

        if (_enfocarEliminar)
        {
            _enfocarEliminar = false;
            await EnfocarBtnEliminarAsync();
        }
    }

    private Task SetEnfocarBusquedaAsync()
    {
        _enfocarBusqueda = true;
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task HandleShortcutAsync(string accion)
    {
        await (accion switch
        {
            "nuevo"         => OnNuevo(),
            "guardar"       => OnGuardar(),
            "borrar"        => OnBorrar(),
            "filtro"        => OnFiltro(),
            "limpiarFiltro" => OnLimpiarFiltro(),
            "escape"        => OnEscape(),
            "busqueda" => SetEnfocarBusquedaAsync(),
            _          => Task.CompletedTask
        });
        await InvokeAsync(StateHasChanged);
    }

    // ── Navegación ─────────────────────────────────────────────
    protected void SeleccionarItem(int id)
    {
        _esNuevo = false;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", id));
    }

    protected void VolverALista()
    {
        _esNuevo = false;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
    }

    public virtual async ValueTask DisposeAsync()
    {
        _prefillado.OnPrefillado -= OnPrefilladoHandler;
        if (_ctsBusqueda is not null) await _ctsBusqueda.CancelAsync();
        _ctsBusqueda?.Dispose();
        if (_jsModule is not null)
        {
            try { await _jsModule.InvokeVoidAsync("unregisterShortcuts"); }
            catch { /* componente desconectado antes de que JS respondiera */ }
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }
}
