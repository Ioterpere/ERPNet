using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Components.Common.Toast;
using ERPNet.Web.Blazor.Client.Components.Pages.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Admin;

[Authorize]
public partial class Menus : PageBase, IAsyncDisposable
{
    [Inject] private IMenusClient MenusClient { get; set; } = default!;
    [Inject] private IRolesClient RolesClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;

    // ── JS interop ─────────────────────────────────────────────
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Menus>? _dotNetRef;
    private ElementReference _refArbol;
    private bool _reinitTree;

    // ── Árbol ──────────────────────────────────────────────────
    private List<NodoMenu> _arbol = [];
    private bool _cargandoArbol = true;
    private string? _errorCarga;

    // ── Drag & drop ────────────────────────────────────────────
    private MovePendiente? _movePendiente;
    private bool _aplicandoMover;
    private string? _errorMover;

    // ── Selección ──────────────────────────────────────────────
    private NodoMenu? _nodoSeleccionado;
    private bool _esNuevo;
    private bool _tieneFormulario => _esNuevo || _nodoSeleccionado is not null;

    // ── Edición ────────────────────────────────────────────────
    private string _editNombre = "";
    private string _editPath = "";
    private string? _editIconClass;
    private bool _guardando;
    private string? _errorGuardar;

    // ── Modal eliminar ─────────────────────────────────────────
    private bool _mostrarModalEliminar;
    private bool _eliminando;
    private string? _errorEliminar { get; set; }

    // ── Roles ──────────────────────────────────────────────────
    private List<RolResponse> _rolesAsignados = [];
    private Dictionary<int, RolResponse> _todosRolesPorId = [];
    private Dictionary<int, RolResponse> _cacheBusquedaRoles = [];
    private int? _addRolId;
    private int _selectorKeyRol;
    private bool _cargandoRoles;

    // ── Nuevo ──────────────────────────────────────────────────
    private string _nuevoNombre = "";
    private string _nuevoPath = "";
    private string? _nuevoIconClass;
    private int? _nuevoPadreId;
    private bool _creando;
    private string? _errorCrear;

    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(CargarArbolAsync(), CargarRolesDiccionarioAsync());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/menus-tree.js");
        }

        if (_reinitTree && _jsModule is not null)
        {
            _reinitTree = false;
            await _jsModule.InvokeVoidAsync("initTree", _dotNetRef, _refArbol);
        }
    }

    // ── Carga de datos ─────────────────────────────────────────
    private async Task CargarArbolAsync()
    {
        _cargandoArbol = true;
        _errorCarga = null;
        try
        {
            var response = await MenusClient.AdminAsync(plataforma: 1);
            _arbol = response.Select(NodoMenu.FromResponse).ToList();
            _reinitTree = true;
        }
        catch
        {
            _errorCarga = "No se pudo cargar la estructura de menús.";
        }
        finally
        {
            _cargandoArbol = false;
        }
    }

    private async Task CargarRolesDiccionarioAsync()
    {
        try
        {
            var resultado = await RolesClient.RolesGETAsync(new PaginacionFilter { Pagina = 0, PorPagina = 200 });
            _todosRolesPorId = resultado.Items.ToDictionary(r => r.Id);
        }
        catch { /* sin roles disponibles */ }
    }

    private async Task CargarRolesNodoAsync(int menuId)
    {
        _cargandoRoles = true;
        _rolesAsignados = [];
        try
        {
            var rolIds = await MenusClient.RolesAllAsync(menuId);
            _rolesAsignados = rolIds
                .Select(id => _todosRolesPorId.TryGetValue(id, out var r) ? r : null)
                .OfType<RolResponse>()
                .ToList();
        }
        catch { /* sin roles */ }
        finally
        {
            _cargandoRoles = false;
        }
    }

    private async Task<IEnumerable<RolResponse>> BuscarRolesAsync(string query, CancellationToken ct)
    {
        var resultado = await RolesClient.RolesGETAsync(new PaginacionFilter
        {
            Pagina    = 0,
            PorPagina = 50,
            Busqueda  = string.IsNullOrWhiteSpace(query) ? null : query
        }, ct);
        _cacheBusquedaRoles = resultado.Items.ToDictionary(r => r.Id);
        return resultado.Items;
    }

    private void AñadirRol()
    {
        if (_addRolId is null) return;
        if (_rolesAsignados.Any(r => r.Id == _addRolId)) return;
        if (!_cacheBusquedaRoles.TryGetValue(_addRolId.Value, out var rol)) return;

        _todosRolesPorId.TryAdd(rol.Id, rol);
        _rolesAsignados.Add(rol);
        _addRolId = null;
        _selectorKeyRol++;
    }

    private void QuitarRol(int rolId) => _rolesAsignados.RemoveAll(r => r.Id == rolId);

    // ── Selección de nodo ──────────────────────────────────────
    private async Task SeleccionarNodo(NodoMenu nodo)
    {
        if (_nodoSeleccionado?.Id == nodo.Id) return;

        _esNuevo = false;
        _nodoSeleccionado = nodo;
        _errorGuardar = null;
        _editNombre = nodo.Nombre;
        _editPath = nodo.Path ?? "";
        _editIconClass = nodo.IconClass;

        await CargarRolesNodoAsync(nodo.Id);
    }

    private void OnNuevo()
    {
        _esNuevo = true;
        _nodoSeleccionado = null;
        _nuevoNombre = "";
        _nuevoPath = "";
        _nuevoIconClass = null;
        _nuevoPadreId = null;
        _errorCrear = null;
    }

    private void Volver()
    {
        _esNuevo = false;
        _nodoSeleccionado = null;
    }

    // ── Drag & drop ────────────────────────────────────────────
    [JSInvokable]
    public async Task OnMoverMenuAsync(int menuId, int? nuevoPadreId, int nuevoIndex)
    {
        _movePendiente = new MovePendiente(menuId, nuevoPadreId, nuevoIndex + 1);
        MoverNodoLocal(menuId, nuevoPadreId, nuevoIndex);
        _errorMover = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task AplicarMoverAsync()
    {
        if (_movePendiente is null) return;

        _aplicandoMover = true;
        _errorMover = null;
        try
        {
            await MenusClient.MoverAsync(_movePendiente.MenuId, new MoverMenuRequest
            {
                MenuPadreId = _movePendiente.NuevoPadreId,
                Orden       = _movePendiente.NuevoOrden
            });

            _movePendiente = null;
            Toast.Exito("Posición guardada correctamente.");
            await CargarArbolAsync();
        }
        catch
        {
            _errorMover = "No se pudo guardar el cambio de posición.";
        }
        finally
        {
            _aplicandoMover = false;
        }
    }

    private async Task CancelarMoverAsync()
    {
        _movePendiente = null;
        _errorMover    = null;
        await CargarArbolAsync();
    }

    // ── Guardar datos ──────────────────────────────────────────
    private async Task GuardarAsync()
    {
        if (_nodoSeleccionado is null) return;

        if (string.IsNullOrWhiteSpace(_editNombre))
        {
            _errorGuardar = "El nombre es obligatorio.";
            return;
        }

        _guardando    = true;
        _errorGuardar = null;
        try
        {
            var menuTask  = MenusClient.MenusPUTAsync(_nodoSeleccionado.Id, new UpdateMenuRequest
            {
                Nombre      = _editNombre,
                Path        = string.IsNullOrWhiteSpace(_editPath) ? null : _editPath,
                IconClass   = _editIconClass,
                CustomClass = _nodoSeleccionado.CustomClass,
                Orden       = _nodoSeleccionado.Orden
            });
            var rolesTask = MenusClient.RolesAsync(_nodoSeleccionado.Id, new AsignarRolesRequest
            {
                RolIds = _rolesAsignados.Select(r => r.Id).ToList()
            });

            await Task.WhenAll(menuTask, rolesTask);

            var updated = await menuTask;
            var (nodo, _) = Encontrar(_arbol, _nodoSeleccionado.Id);
            if (nodo is not null)
            {
                nodo.Nombre    = updated.Nombre;
                nodo.Path      = updated.Path;
                nodo.IconClass = updated.IconClass;
                _nodoSeleccionado = nodo;
            }

            Toast.Exito("Menú guardado correctamente.");
        }
        catch
        {
            _errorGuardar = "No se pudo guardar el menú.";
        }
        finally
        {
            _guardando = false;
        }
    }

    // ── Crear ──────────────────────────────────────────────────
    private async Task CrearAsync()
    {
        if (string.IsNullOrWhiteSpace(_nuevoNombre))
        {
            _errorCrear = "El nombre es obligatorio.";
            return;
        }

        _creando    = true;
        _errorCrear = null;

        int orden = _nuevoPadreId is null
            ? _arbol.Count + 1
            : (Encontrar(_arbol, _nuevoPadreId.Value).nodo?.SubMenus.Count ?? 0) + 1;

        try
        {
            await MenusClient.MenusPOSTAsync(new CreateMenuRequest
            {
                Nombre      = _nuevoNombre,
                Path        = string.IsNullOrWhiteSpace(_nuevoPath) ? null : _nuevoPath,
                IconClass   = _nuevoIconClass,
                Orden       = orden,
                Plataforma  = 1,
                MenuPadreId = _nuevoPadreId,
                RolIds      = []
            });

            Toast.Exito("Menú creado correctamente.");
            _esNuevo = false;
            await CargarArbolAsync();
        }
        catch
        {
            _errorCrear = "No se pudo crear el menú.";
        }
        finally
        {
            _creando = false;
        }
    }

    // ── Eliminar ───────────────────────────────────────────────
    private void AbrirModalEliminar()
    {
        _mostrarModalEliminar = true;
        _errorEliminar        = null;
    }

    private async Task EliminarAsync()
    {
        if (_nodoSeleccionado is null) return;

        _eliminando    = true;
        _errorEliminar = null;
        try
        {
            await MenusClient.MenusDELETEAsync(_nodoSeleccionado.Id);
            _mostrarModalEliminar = false;
            _nodoSeleccionado     = null;
            Toast.Exito("Menú eliminado correctamente.");
            await CargarArbolAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorEliminar = "No se puede eliminar un menú que tiene submenús.";
        }
        catch
        {
            _errorEliminar = "No se pudo eliminar el menú.";
        }
        finally
        {
            _eliminando = false;
        }
    }

    // ── Árbol: búsqueda y manipulación ────────────────────────
    private (NodoMenu? nodo, List<NodoMenu>? lista) Encontrar(List<NodoMenu> lista, int id)
    {
        foreach (var n in lista)
        {
            if (n.Id == id) return (n, lista);
            var (found, fl) = Encontrar(n.SubMenus, id);
            if (found is not null) return (found, fl);
        }
        return (null, null);
    }

    private void MoverNodoLocal(int menuId, int? nuevoPadreId, int nuevoIndex)
    {
        var (nodo, listaOrigen) = Encontrar(_arbol, menuId);
        if (nodo is null || listaOrigen is null) return;

        listaOrigen.Remove(nodo);
        RecalcularOrden(listaOrigen);

        List<NodoMenu> listaDest;
        if (nuevoPadreId is null)
        {
            listaDest = _arbol;
        }
        else
        {
            var (padre, _) = Encontrar(_arbol, nuevoPadreId.Value);
            if (padre is null) { listaOrigen.Add(nodo); return; }
            listaDest = padre.SubMenus;
        }

        var idx = Math.Clamp(nuevoIndex, 0, listaDest.Count);
        listaDest.Insert(idx, nodo);
        nodo.MenuPadreId = nuevoPadreId;
        RecalcularOrden(listaDest);
    }

    private IEnumerable<(NodoMenu Nodo, int Nivel)> AplanarArbol(List<NodoMenu> lista, int nivel = 0)
    {
        foreach (var n in lista)
        {
            yield return (n, nivel);
            foreach (var sub in AplanarArbol(n.SubMenus, nivel + 1))
                yield return sub;
        }
    }

    private static void RecalcularOrden(List<NodoMenu> lista)
    {
        for (int i = 0; i < lista.Count; i++) lista[i].Orden = i + 1;
    }

    // ── Cleanup ────────────────────────────────────────────────
    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try { await _jsModule.InvokeVoidAsync("dispose"); } catch { }
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }

    // ── Modelos locales ────────────────────────────────────────
    private record MovePendiente(int MenuId, int? NuevoPadreId, int NuevoOrden);

    private class NodoMenu
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int? MenuPadreId { get; set; }
        public string? Path { get; set; }
        public string? IconClass { get; set; }
        public string? CustomClass { get; set; }
        public int Orden { get; set; }
        public List<NodoMenu> SubMenus { get; set; } = [];

        public static NodoMenu FromResponse(MenuResponse r) => new()
        {
            Id          = r.Id,
            Nombre      = r.Nombre,
            MenuPadreId = r.MenuPadreId,
            Path        = r.Path,
            IconClass   = r.IconClass,
            CustomClass = r.CustomClass,
            Orden       = r.Orden,
            SubMenus    = r.SubMenus.Select(FromResponse).ToList()
        };

    }
}
