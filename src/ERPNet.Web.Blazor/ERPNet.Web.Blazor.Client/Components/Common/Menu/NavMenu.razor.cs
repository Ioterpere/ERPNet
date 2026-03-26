using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace ERPNet.Web.Blazor.Client.Components.Common.Menu;

public partial class NavMenu : IDisposable
{
    [Inject] private IMenusClient MenusClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private PersistentComponentState ApplicationState { get; set; } = default!;
    [Inject] private MenuStateService MenuState { get; set; } = default!;
    [Inject] private PermisosService Permisos { get; set; } = default!;
    [Inject] private EmpresaStateService EmpresaState { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    private ICollection<MenuResponse>? _menus;
    private readonly HashSet<int> _expanded = [];

    private int? _empresaActualId;
    private string? _empresaActualNombre;
    private List<EmpresaItem> _empresas = [];
    private bool _cargandoEmpresas;

    private PersistingComponentStateSubscription _persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        _persistingSubscription = ApplicationState.RegisterOnPersisting(PersistirMenus);

        // Restaurar menús ANTES del primer await: Blazor renderiza el componente en el
        // primer punto de suspensión, así que _menus debe tener datos para evitar el skeleton.
        if (ApplicationState.TryTakeFromJson<List<MenuResponse>>("nav-menus", out var persisted) && persisted is { Count: > 0 })
        {
            MenuState.Seed(persisted);
            _menus = persisted;
            AutoExpandActive();
        }

        // Primer await — Blazor renderiza aquí. _menus ya tiene datos → sin skeleton.
        var state = await AuthStateTask;
        var empresaIdStr = state.User.FindFirst("empresa_id")?.Value;
        if (int.TryParse(empresaIdStr, out var eid))
            _empresaActualId = eid;

        _empresaActualNombre = state.User.FindFirst("empresa_nombre")?.Value;

        if (_menus is null)
        {
            if (RendererInfo.IsInteractive)
            {
                _menus = await MenuState.ObtenerAsync(MenusClient);
                AutoExpandActive();
            }
            else
            {
                await CargarMenusAsync();
            }
        }

        if (RendererInfo.IsInteractive)
            await CargarEmpresasAsync();
    }

    private Task PersistirMenus()
    {
        // Solo persistir si SSR cargó menús — si está vacío, WASM hará su propia llamada
        if (_menus is { Count: > 0 })
            ApplicationState.PersistAsJson("nav-menus", _menus);
        return Task.CompletedTask;
    }

    public void Dispose() => _persistingSubscription.Dispose();

    private async Task CargarMenusAsync()
    {
        try
        {
            _menus = await MenusClient.MenusAllAsync();
            AutoExpandActive();
        }
        catch
        {
            _menus = [];
        }
    }

    private async Task CargarEmpresasAsync()
    {
        try
        {
            var empresas = await Http.GetFromJsonAsync<List<EmpresaItem>>("api/empresas/mis-empresas");
            if (empresas is { Count: > 0 })
            {
                _empresas = empresas;
                _empresaActualNombre = _empresas.FirstOrDefault(e => e.Id == _empresaActualId)?.Nombre
                                    ?? _empresas.FirstOrDefault()?.Nombre;
            }
        }
        catch { /* no interrumpir la navegación si falla */ }
        finally { _cargandoEmpresas = false; }
    }

    private async Task CambiarEmpresaAsync(int empresaId)
    {
        if (empresaId == _empresaActualId) return;

        _cargandoEmpresas = true;

        try
        {
            var content = new FormUrlEncodedContent(
                new[] { new KeyValuePair<string, string>(key: "empresaId", empresaId.ToString()) });
            var response = await Http.PostAsync("bff/cambiar-empresa", content);
            if (!response.IsSuccessStatusCode) return;

            // Actualizar estado de empresa (notifica MainLayout, PageBase, etc.)
            var nombre = _empresas.FirstOrDefault(e => e.Id == empresaId)?.Nombre ?? "";
            _empresaActualId = empresaId;
            _empresaActualNombre = nombre;
            EmpresaState.Cambiar(empresaId, nombre);

            // Invalidar cachés empresa-dependientes y re-fetch menús
            MenuState.Invalidar();
            Permisos.Invalidar();
            _menus = null; // muestra skeleton mientras carga
            _menus = await MenuState.ObtenerAsync(MenusClient);
            _expanded.Clear();
            AutoExpandActive();

            Nav.NavigateTo("/", forceLoad: false);
        }
        catch { /* ignorar errores de red */ }
        finally { _cargandoEmpresas = false; }
    }

    private void AutoExpandActive()
    {
        if (_menus is null) return;
        var currentPath = "/" + Nav.ToBaseRelativePath(Nav.Uri).Split('?')[0];
        foreach (var menu in _menus.Where(m => m.SubMenus?.Count > 0))
        {
            foreach (var sub in menu.SubMenus!)
            {
                if (sub.Path == currentPath)
                {
                    _expanded.Add(menu.Id);
                }
                else if (sub.SubMenus?.Count > 0)
                {
                    if (sub.SubMenus.Any(s => s.Path == currentPath))
                    {
                        _expanded.Add(menu.Id);
                        _expanded.Add(sub.Id);
                    }
                }
            }
        }
    }

    private void Toggle(int id)
    {
        if (!_expanded.Remove(id))
            _expanded.Add(id);
    }

    private sealed record EmpresaItem(int Id, string Nombre, string? Cif, bool Activo);
}
