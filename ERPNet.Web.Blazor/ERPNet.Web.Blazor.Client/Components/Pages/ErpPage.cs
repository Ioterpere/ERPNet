using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ERPNet.Web.Blazor.Client.Components.Pages;

/// <summary>
/// Clase base para páginas del Erp.
/// Gestiona: parámetro Id, estado de paginación/búsqueda,
/// navegación entre registros y atajos de teclado (Alt+N, Ctrl+S, Alt+Supr).
/// </summary>
[Authorize]
public abstract class ErpPage : ComponentBase, IAsyncDisposable
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ToastService Toast { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "id")]
    public int? Id { get; set; }

    // ── Estado compartido ──────────────────────────────────────
    protected bool _esNuevo;
    protected int _pagina = 1;
    protected string _busqueda = string.Empty;

    protected virtual int PorPagina => 15;

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

    protected virtual Task EnfocarPrimerCampoNuevoAsync() => Task.CompletedTask;

    // ── Abstracts: paginación ──────────────────────────────────
    protected abstract int? TotalPaginas { get; }

    // ── Abstracts: ciclo de vida de datos ─────────────────────
    protected abstract Task CargarListaAsync();
    protected abstract Task CargarDetalleAsync(int id);
    protected abstract Task LimpiarDetalleAsync();

    // ── Abstracts: acciones de teclado ────────────────────────
    protected abstract Task OnNuevo();
    protected abstract Task OnGuardar();
    protected abstract Task OnBorrar();

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

    protected async Task PaginaAnterior()
    {
        if (_pagina <= 1) return;
        _pagina--;
        await CargarListaAsync();
    }

    protected async Task PaginaSiguiente()
    {
        if (TotalPaginas is null || _pagina >= TotalPaginas) return;
        _pagina++;
        await CargarListaAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try { await _jsModule.InvokeVoidAsync("unregisterShortcuts"); }
            catch { /* componente desconectado antes de que JS respondiera */ }
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }
}
