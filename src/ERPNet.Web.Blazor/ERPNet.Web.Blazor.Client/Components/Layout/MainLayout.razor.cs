using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Security.Claims;

namespace ERPNet.Web.Blazor.Client.Components.Layout;

public partial class MainLayout : IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private PermisosService Permisos { get; set; } = default!;
    [Inject] private MenuStateService MenuState { get; set; } = default!;
    [Inject] private EmpresaStateService EmpresaState { get; set; } = default!;
    [Inject] private IAiClient AiClient { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    private string? _nombre;
    private string? _email;
    private string? _empresaNombre;
    private bool _sidebarCollapsed;
    private bool _cleanupPending;
    private bool _tieneAsistenteIa;
    private ICollection<string>? _herramientas;
    private DotNetObjectReference<MainLayout>? _dotNetRef;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthStateTask;
        _nombre = state.User.FindFirst(ClaimTypes.Name)?.Value;
        _email  = state.User.FindFirst(ClaimTypes.Email)?.Value;
        _empresaNombre = state.User.FindFirst("empresa_nombre")?.Value;
        EmpresaState.Inicializar(
            int.TryParse(state.User.FindFirst("empresa_id")?.Value, out var eid) ? eid : null,
            _empresaNombre);
        EmpresaState.OnCambio += OnEmpresaCambiada;
        EmpresaState.OnEmpresasLoaded += OnEmpresasLoaded;
        if (!RendererInfo.IsInteractive) return;
        if (await Permisos.TieneAcceso(RecursoCodigo.AsistenteIa))
        {
            try
            {
                await AiClient.AccesoAsync();
                _herramientas = await AiClient.HerramientasAsync();
                _tieneAsistenteIa = true;
            }
            catch { }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var val = await JS.InvokeAsync<string?>("localStorage.getItem", "sidebar-collapsed");
                _sidebarCollapsed = val == "true";
            }
            catch { /* localStorage no disponible (modo privado, storage desactivado) */ }

            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("sidebar.registerShortcut", _dotNetRef);
            await JS.InvokeVoidAsync("chat.registerShortcut");

            _cleanupPending = true;
            StateHasChanged();
        }
        else if (_cleanupPending)
        {
            _cleanupPending = false;
            // El atributo pre-render ya no es necesario: Blazor tiene el estado correcto
            await JS.InvokeVoidAsync("sidebar.removeSidebarInit");
        }
    }

    private async Task ToggleSidebar()
    {
        _sidebarCollapsed = !_sidebarCollapsed;
        try
        {
            await JS.InvokeVoidAsync("localStorage.setItem", "sidebar-collapsed", _sidebarCollapsed ? "true" : "false");
        }
        catch { /* localStorage no disponible */ }
    }

    [JSInvokable]
    public async Task ToggleSidebarFromJs()
    {
        await ToggleSidebar();
        await InvokeAsync(StateHasChanged);
    }

    private void OnEmpresaCambiada()
    {
        _empresaNombre = EmpresaState.EmpresaNombre;
        InvokeAsync(StateHasChanged);
    }

    private void OnEmpresasLoaded() => InvokeAsync(StateHasChanged);

    private async Task CambiarEmpresaAsync(int empresaId)
    {
        if (empresaId == EmpresaState.EmpresaId) return;
        try
        {
            var content = new FormUrlEncodedContent(
                [new KeyValuePair<string, string>("empresaId", empresaId.ToString())]);
            var response = await Http.PostAsync("bff/cambiar-empresa", content);
            if (!response.IsSuccessStatusCode) return;

            var nombre = EmpresaState.Empresas.FirstOrDefault(e => e.Id == empresaId)?.Nombre ?? "";
            EmpresaState.Cambiar(empresaId, nombre); // NavMenu reacciona vía OnCambio → recarga menús
        }
        catch { /* ignorar errores de red */ }
    }

    public async ValueTask DisposeAsync()
    {
        EmpresaState.OnCambio -= OnEmpresaCambiada;
        EmpresaState.OnEmpresasLoaded -= OnEmpresasLoaded;
        try { await JS.InvokeVoidAsync("sidebar.unregisterShortcut"); } catch { }
        try { await JS.InvokeVoidAsync("chat.unregisterShortcut"); } catch { }
        _dotNetRef?.Dispose();
    }
}
