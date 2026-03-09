using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace ERPNet.Web.Blazor.Client.Components.Pages;

/// <summary>
/// Clase base para páginas del Erp.
/// Gestiona: parámetro Id, estado de paginación/búsqueda,
/// navegación entre registros y atajos de teclado.
/// </summary>
[Authorize]
public abstract class ErpPage : ComponentBase, IAsyncDisposable
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ToastService Toast { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? AuthStateTask { get; set; }

    protected string NombreEmpresa =>
        AuthStateTask?.IsCompletedSuccessfully == true
            ? AuthStateTask.Result.User.FindFirst("empresa_nombre")?.Value ?? "ERPNet"
            : "ERPNet";

    [SupplyParameterFromQuery(Name = "id")]
    public int? Id { get; set; }

    // ── Estado compartido ──────────────────────────────────────
    protected bool _esNuevo;
    protected string _busqueda = string.Empty;
    protected ElementReference _refBusqueda;
    private bool _enfocarBusqueda;

    private CancellationTokenSource? _ctsBusqueda;

    protected async Task OnBusquedaInputAsync(ChangeEventArgs e)
    {
        _busqueda = e.Value?.ToString() ?? string.Empty;
        _ctsBusqueda?.Cancel();
        _ctsBusqueda = new CancellationTokenSource();
        try
        {
            await Task.Delay(300, _ctsBusqueda.Token);
            await CargarListaAsync();
        }
        catch (OperationCanceledException) { }
    }

    // ── Modal de eliminación ───────────────────────────────────
    protected bool _mostrarModalEliminar;
    protected ElementReference _refBtnEliminar;
    private bool _enfocarEliminar;

    protected void AbrirModalEliminar()
    {
        _mostrarModalEliminar = true;
        _enfocarEliminar      = true;
    }

    // ── Foco en formulario de creación ─────────────────────────
    protected bool _enfocarNuevo;

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

    protected virtual Task OnFiltro()       => Task.CompletedTask;
    protected virtual Task OnLimpiarFiltro() => Task.CompletedTask;
    protected virtual Task OnEscape()
    {
        _mostrarModalEliminar = false;
        return Task.CompletedTask;
    }

    // ── Ciclo de vida ──────────────────────────────────────────
    private int? _idActual;

    protected override async Task OnParametersSetAsync()
    {
        if (Id == _idActual) return;
        _idActual = Id;

        if (Id.HasValue)
        {
            _esNuevo = false;
            await CargarDetalleAsync(Id.Value);
        }
        else
        {
            // _esNuevo NO se resetea aquí: OnNuevo() lo pone a true
            // antes de navegar, y esta rama solo limpia el detalle.
            await LimpiarDetalleAsync();
        }
    }

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ErpPage>? _dotNetRef;

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

        if (_enfocarFiltro)
        {
            _enfocarFiltro = false;
            await EnfocarPrimerCampoFiltroAsync();
        }

        if (_enfocarBusqueda)
        {
            _enfocarBusqueda = false;
            await _refBusqueda.FocusAsync();
        }

        if (_enfocarEliminar)
        {
            _enfocarEliminar = false;
            await _refBtnEliminar.FocusAsync();
        }
    }

    [JSInvokable]
    public async Task HandleShortcutAsync(string accion)
    {
        await (accion switch
        {
            "nuevo"   => OnNuevo(),
            "guardar" => OnGuardar(),
            "borrar"  => OnBorrar(),
            "filtro"        => OnFiltro(),
            "limpiarFiltro" => OnLimpiarFiltro(),
            "escape"   => OnEscape(),
            "busqueda" => Task.FromResult(_enfocarBusqueda = true),
            _         => Task.CompletedTask
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

    public async ValueTask DisposeAsync()
    {
        _ctsBusqueda?.Cancel();
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
