using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Components.Common;
using ERPNet.Web.Blazor.Client.Components.Common.Tabs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.Net.Http.Json;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Admin;

public partial class Roles
{
    [Inject] private IRolesClient RolesClient { get; set; } = default!;
    [Inject] private IUsuariosClient UsuariosClient { get; set; } = default!;
    [Inject] private IEmpresasClient EmpresasClient { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    // ── Lista ──────────────────────────────────────────────────
    private VirtualList<RolResponse>? _refLista;
    private int? _totalItems;

    // ── Tabs ───────────────────────────────────────────────────
    private static readonly TabItem[] _tabsRol =
    [
        new("usuarios", "Usuarios", "bi-people"),
        new("permisos", "Permisos", "bi-shield"),
    ];

    // ── Estado ─────────────────────────────────────────────────
    private bool _mostrarModalEditar;
    private bool _eliminando;
    private string? _errorEliminar { get; set; }

    // ── Recursos (cargados una vez) ────────────────────────────
    private List<RecursoResponse> _todosRecursos = [];

    // ── Detalle ────────────────────────────────────────────────
    private RolResponse? _rolDetalle;
    private bool _cargandoDetalle;
    private string? _errorDetalle;

    // ── Tab Datos ──────────────────────────────────────────────
    private string _editNombre = string.Empty;
    private string _editDescripcion = string.Empty;
    private bool _guardandoDatos;
    private string? _errorDatos;
    private bool _enfocarModalEditar;

    // ── Tab Permisos ───────────────────────────────────────────
    private List<PermisoEditModel> _permisosEdit = [];
    private int _recursoSeleccionadoId;
    private bool _guardandoPermisos;
    private string? _errorPermisos;

    private IEnumerable<RecursoResponse> _recursosDisponibles =>
        _todosRecursos.Where(r => !_permisosEdit.Any(p => p.RecursoId == r.Id));

    // ── Tab Usuarios ───────────────────────────────────────────
    private List<AsignacionUsuario> _asignacionesUsuario = [];
    private Dictionary<int, UsuarioResponse> _usuariosPorId = [];
    private List<EmpresaResponse> _todasEmpresas = [];
    private Dictionary<int, UsuarioResponse> _cacheBusquedaUsuarios = [];
    private int? _addUsuarioId;
    private int _selectorKeyUsuario;
    private string _addEmpresaStr = "";
    private bool _guardandoUsuarios;
    private string? _errorUsuarios;

    private int? AddEmpresaIdUsuario => string.IsNullOrEmpty(_addEmpresaStr) ? null
        : int.TryParse(_addEmpresaStr, out var id) ? id : null;

    // ── Formulario edición ─────────────────────────────────────
    private ElementReference _refEditNombre;

    // ── Formulario creación ────────────────────────────────────
    private ElementReference _refNuevoNombre;
    private string _nuevoNombre = string.Empty;
    private string _nuevoDescripcion = string.Empty;
    private bool _creando;
    private string? _errorCrear;

    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(CargarTodosRecursosAsync(), CargarTodosUsuariosAsync(), CargarTodasEmpresasAsync());
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        if (_refLista is not null)
            await _refLista.RefreshAsync();
    }

    internal async ValueTask<ItemsProviderResult<RolResponse>> CargarItemsAsync(ItemsProviderRequest request)
    {
        try
        {
            var resultado = await RolesClient.RolesGETAsync(new PaginacionFilter
            {
                Pagina    = request.StartIndex,
                PorPagina = request.Count,
                Busqueda  = string.IsNullOrWhiteSpace(_busqueda) ? null : _busqueda
            });
            return new(resultado.Items, resultado.TotalRegistros);
        }
        catch
        {
            return new([], 0);
        }
    }

    protected override async Task CargarDetalleAsync(int id)
    {
        _cargandoDetalle = true;
        _errorDetalle = null;
        LimpiarFeedbackDetalle();

        try
        {
            var rolTask          = RolesClient.RolesGET2Async(id);
            var permisosTask     = RolesClient.PermisosAllAsync(id);
            var asignacionesTask = Http.GetFromJsonAsync<List<AsignacionUsuario>>($"api/roles/{id}/usuarios/todas");
            await Task.WhenAll(rolTask, permisosTask, asignacionesTask);

            _rolDetalle = await rolTask;

            _permisosEdit = (await permisosTask).Select(p => new PermisoEditModel
            {
                RecursoId = p.RecursoId,
                Codigo    = p.RecursoCodigo,
                CanCreate = p.CanCreate,
                CanEdit   = p.CanEdit,
                CanDelete = p.CanDelete,
                Alcance   = p.Alcance
            }).ToList();

            _asignacionesUsuario = (await asignacionesTask) ?? [];

            _editNombre      = _rolDetalle.Nombre;
            _editDescripcion = _rolDetalle.Descripcion ?? string.Empty;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _errorDetalle = "Rol no encontrado.";
        }
        catch
        {
            _errorDetalle = "No se pudo cargar la información del rol.";
        }
        finally
        {
            _cargandoDetalle = false;
        }
    }

    protected override Task LimpiarDetalleAsync()
    {
        LimpiarDetalle();
        return Task.CompletedTask;
    }

    protected override Task OnNuevo()
    {
        _esNuevo          = true;
        _nuevoNombre      = string.Empty;
        _nuevoDescripcion = string.Empty;
        _errorCrear       = null;
        _enfocarNuevo     = true;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        return Task.CompletedTask;
    }

    protected override async Task EnfocarPrimerCampoNuevoAsync()
        => await _refNuevoNombre.FocusAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_enfocarModalEditar)
        {
            _enfocarModalEditar = false;
            await _refEditNombre.FocusAsync();
        }
    }

    protected override Task OnGuardar()
    {
        if (_mostrarModalEditar) return GuardarDatosAsync();
        if (_esNuevo) return CrearRolAsync();
        return (Tab ?? "usuarios") switch
        {
            "usuarios" => GuardarUsuariosAsync(),
            "permisos" => GuardarPermisosAsync(),
            _          => Task.CompletedTask
        };
    }

    protected override Task OnBorrar()
    {
        if (!Id.HasValue || _rolDetalle is null) return Task.CompletedTask;
        AbrirModalEliminar();
        return Task.CompletedTask;
    }

    private void AbrirModalEditar()
    {
        _errorDatos         = null;
        _mostrarModalEditar = true;
        _enfocarModalEditar = true;
    }

    // ── Carga auxiliar ─────────────────────────────────────────
    private async Task CargarTodosRecursosAsync()
    {
        try
        {
            var recursos = await RolesClient.RecursosAsync();
            _todosRecursos = recursos.ToList();
        }
        catch { /* sin recursos */ }
    }

    private async Task CargarTodosUsuariosAsync()
    {
        try
        {
            var resultado = await UsuariosClient.UsuariosGETAsync(new PaginacionFilter { Pagina = 0, PorPagina = 200 });
            _usuariosPorId = resultado.Items.ToDictionary(u => u.Id);
        }
        catch { /* sin usuarios */ }
    }

    private async Task CargarTodasEmpresasAsync()
    {
        try
        {
            var resultado = await EmpresasClient.EmpresasGETAsync(new PaginacionFilter { Pagina = 0, PorPagina = 200 });
            _todasEmpresas = resultado.Items.ToList();
        }
        catch { /* sin empresas */ }
    }

    private async Task<IEnumerable<UsuarioResponse>> BuscarUsuariosAsync(string query, CancellationToken ct)
    {
        var resultado = await UsuariosClient.UsuariosGETAsync(new PaginacionFilter
        {
            Pagina    = 0,
            PorPagina = 50,
            Busqueda  = string.IsNullOrWhiteSpace(query) ? null : query
        }, ct);
        _cacheBusquedaUsuarios = resultado.Items.ToDictionary(u => u.Id);
        return resultado.Items;
    }

    // ── Helpers ────────────────────────────────────────────────
    private void LimpiarDetalle()
    {
        _rolDetalle = null;
        _permisosEdit.Clear();
        _asignacionesUsuario.Clear();
        LimpiarFeedbackDetalle();
    }

    private void LimpiarFeedbackDetalle()
    {
        _errorDatos    = null;
        _errorPermisos = null;
        _errorUsuarios = null;
    }

    private void AñadirPermiso()
    {
        if (_recursoSeleccionadoId == 0) return;
        var recurso = _todosRecursos.FirstOrDefault(r => r.Id == _recursoSeleccionadoId);
        if (recurso is null || _permisosEdit.Any(p => p.RecursoId == recurso.Id)) return;

        _permisosEdit.Add(new PermisoEditModel { RecursoId = recurso.Id, Codigo = recurso.Codigo });
        _recursoSeleccionadoId = 0;
    }

    private void QuitarPermiso(PermisoEditModel permiso) => _permisosEdit.Remove(permiso);

    private void AñadirAsignacionUsuario()
    {
        if (_addUsuarioId is null) return;
        var empresaId = AddEmpresaIdUsuario;
        if (_asignacionesUsuario.Any(a => a.UsuarioId == _addUsuarioId && a.EmpresaId == empresaId)) return;

        if (_cacheBusquedaUsuarios.TryGetValue(_addUsuarioId.Value, out var usuario))
            _usuariosPorId.TryAdd(usuario.Id, usuario);

        _asignacionesUsuario.Add(new AsignacionUsuario { UsuarioId = _addUsuarioId.Value, EmpresaId = empresaId });
        _addUsuarioId = null;
        _selectorKeyUsuario++;
    }

    private void EliminarAsignacionUsuario(int usuarioId, int? empresaId)
        => _asignacionesUsuario.RemoveAll(a => a.UsuarioId == usuarioId && a.EmpresaId == empresaId);

    // ── Acciones ───────────────────────────────────────────────
    private async Task GuardarDatosAsync()
    {
        if (_rolDetalle is null || !Id.HasValue) return;

        _errorDatos    = null;
        _guardandoDatos = true;
        try
        {
            await RolesClient.RolesPUTAsync(Id.Value, new UpdateRolRequest
            {
                Nombre      = _editNombre,
                Descripcion = string.IsNullOrWhiteSpace(_editDescripcion) ? null : _editDescripcion
            });

            Toast.Exito("Datos guardados correctamente.");
            _mostrarModalEditar = false;
            _rolDetalle = await RolesClient.RolesGET2Async(Id.Value);
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorDatos = "Ya existe un rol con ese nombre.";
        }
        catch
        {
            _errorDatos = "No se pudieron guardar los cambios.";
        }
        finally
        {
            _guardandoDatos = false;
        }
    }

    private async Task GuardarPermisosAsync()
    {
        if (!Id.HasValue) return;

        _errorPermisos    = null;
        _guardandoPermisos = true;
        try
        {
            await RolesClient.PermisosAsync(Id.Value, new SetPermisosRolRequest
            {
                Permisos = _permisosEdit.Select(p => new PermisoRolRecursoDto
                {
                    RecursoId = p.RecursoId,
                    CanCreate = p.CanCreate,
                    CanEdit   = p.CanEdit,
                    CanDelete = p.CanDelete,
                    Alcance   = p.Alcance
                }).ToList()
            });

            Toast.Exito("Permisos guardados correctamente.");
        }
        catch
        {
            _errorPermisos = "No se pudieron guardar los permisos.";
        }
        finally
        {
            _guardandoPermisos = false;
        }
    }

    private async Task GuardarUsuariosAsync()
    {
        if (!Id.HasValue) return;

        _errorUsuarios     = null;
        _guardandoUsuarios = true;
        try
        {
            var response = await Http.PutAsJsonAsync(
                $"api/roles/{Id.Value}/usuarios/todas",
                _asignacionesUsuario.Select(a => new { a.UsuarioId, a.EmpresaId }).ToList());

            if (!response.IsSuccessStatusCode)
            {
                _errorUsuarios = "No se pudieron actualizar los usuarios.";
                return;
            }
            Toast.Exito("Usuarios actualizados correctamente.");
        }
        catch
        {
            _errorUsuarios = "No se pudieron actualizar los usuarios.";
        }
        finally
        {
            _guardandoUsuarios = false;
        }
    }

    private async Task EliminarRolAsync()
    {
        if (!Id.HasValue) return;

        _errorEliminar = null;
        _eliminando    = true;
        try
        {
            await RolesClient.RolesDELETEAsync(Id.Value);
            _mostrarModalEliminar = false;
            await CargarListaAsync();
            Toast.Exito("Rol eliminado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        }
        catch
        {
            _errorEliminar = "No se pudo eliminar el rol.";
        }
        finally
        {
            _eliminando = false;
        }
    }

    private async Task CrearRolAsync()
    {
        _errorCrear = null;

        if (string.IsNullOrWhiteSpace(_nuevoNombre))
        {
            _errorCrear = "El nombre es obligatorio.";
            return;
        }

        _creando = true;
        try
        {
            var creado = await RolesClient.RolesPOSTAsync(new CreateRolRequest
            {
                Nombre      = _nuevoNombre,
                Descripcion = string.IsNullOrWhiteSpace(_nuevoDescripcion) ? null : _nuevoDescripcion
            });

            await CargarListaAsync();
            Toast.Exito("Rol creado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", creado.Id));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorCrear = "Ya existe un rol con ese nombre.";
        }
        catch
        {
            _errorCrear = "No se pudo crear el rol.";
        }
        finally
        {
            _creando = false;
        }
    }

    // ── Modelo local para edición de permisos ──────────────────
    private class PermisoEditModel
    {
        public int RecursoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public int Alcance { get; set; }
    }

    // ── Modelos locales para usuarios ──────────────────────────
    private record AsignacionUsuario
    {
        public int UsuarioId { get; init; }
        public int? EmpresaId { get; init; }
    }

}
