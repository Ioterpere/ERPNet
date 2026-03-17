using System.Net.Http.Json;
using System.Text.Json;
using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Components.Common;
using ERPNet.Web.Blazor.Client.Components.Common.Tabs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Admin;

public partial class Usuarios
{
    [Inject] private IUsuariosClient UsuariosClient { get; set; } = default!;
    [Inject] private IEmpleadosClient EmpleadosClient { get; set; } = default!;
    [Inject] private IRolesClient RolesClient { get; set; } = default!;
    [Inject] private IEmpresasClient EmpresasClient { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    // ── Lista ──────────────────────────────────────────────────
    private VirtualList<UsuarioResponse>? _refLista;
    private int? _totalItems;
    private Dictionary<int, EmpleadoResponse> _empleadosPorId = [];

    // ── Roles (todos) ──────────────────────────────────────────
    private List<RolResponse> _todosRoles = [];

    // ── Empresas (todas) ───────────────────────────────────────
    private List<EmpresaResponse> _todasEmpresas = [];

    // ── Tabs ───────────────────────────────────────────────────
    private static readonly TabItem[] _tabsUsuario =
    [
        new("datos",    "Datos",    "bi-person-circle"),
        new("empresas", "Empresas", "bi-building"),
        new("roles",    "Roles",    "bi-shield"),
    ];

    // ── Detalle ────────────────────────────────────────────────
    private UsuarioResponse? _usuarioDetalle;
    private EmpleadoResponse? _empleadoDetalle;
    private HashSet<int> _empresasUsuario = [];
    private bool _cargandoDetalle;
    private string? _errorDetalle;

    // ── Formulario edición ─────────────────────────────────────
    private string _editEmail = string.Empty;
    private bool _editActivo;
    private DateTime? _editCaducidadDate;
    private bool _guardandoDatos;
    private string? _errorDatos;

    // ── Roles edición ──────────────────────────────────────────
    private List<AsignacionRol> _asignaciones = [];
    private string _addContexto = "";   // "" = Global, "123" = empresa ID
    private string _addRolStr = "";     // "" = sin selección, "123" = rol ID
    private ElementReference _refAddRol;
    private bool _guardandoRoles;
    private string? _errorRoles;

    private int? AddEmpresaId => string.IsNullOrEmpty(_addContexto) ? null
        : int.TryParse(_addContexto, out var id) ? id : null;
    private int? AddRolId => int.TryParse(_addRolStr, out var id) ? id : null;

    // ── Empresas edición ───────────────────────────────────────
    private bool _guardandoEmpresas;
    private string? _errorEmpresas;

    // ── Reset contraseña ───────────────────────────────────────
    private bool _reseteandoContrasena;
    private string? _errorReset;

    // ── Eliminar usuario ───────────────────────────────────────
    private bool _eliminando;
    private string? _errorEliminar { get; set; }

    // ── Formulario creación ────────────────────────────────────
    private ElementReference _refNuevoEmail;
    private string _nuevoEmail = string.Empty;
    private int? _nuevoEmpleadoId;
    private HashSet<int> _rolesNuevo = [];
    private bool _creando;
    private string? _errorCrear;


    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Task.WhenAll(CargarTodosRolesAsync(), CargarTodasEmpresasAsync());
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        _empleadosPorId = [];
        if (_refLista is not null)
            await _refLista.RefreshAsync();
    }

    internal async ValueTask<ItemsProviderResult<UsuarioResponse>> CargarItemsAsync(ItemsProviderRequest request)
    {
        try
        {
            var resultado = await UsuariosClient.UsuariosGETAsync(new PaginacionFilter
            {
                Pagina    = request.StartIndex,
                PorPagina = request.Count,
                Busqueda  = string.IsNullOrWhiteSpace(_busqueda) ? null : _busqueda
            });

            var ids = resultado.Items.Select(u => u.EmpleadoId).Distinct();
            var empleados = await Task.WhenAll(ids.Select(async id =>
            {
                try { return await EmpleadosClient.EmpleadosGET2Async(id); }
                catch { return null; }
            }));
            foreach (var emp in empleados.OfType<EmpleadoResponse>())
                _empleadosPorId[emp.Id] = emp;

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
            var usuarioTask     = UsuariosClient.UsuariosGET2Async(id);
            var asignacionesTask = Http.GetFromJsonAsync<List<AsignacionRol>>($"api/usuarios/{id}/roles/todas");
            var empresasTask    = UsuariosClient.EmpresasAllAsync(id);
            await Task.WhenAll(usuarioTask, asignacionesTask, empresasTask);

            _usuarioDetalle  = await usuarioTask;
            _asignaciones    = (await asignacionesTask) ?? [];
            _empresasUsuario = ((await empresasTask) ?? []).ToHashSet();

            _empleadoDetalle = await EmpleadosClient.EmpleadosGET2Async(_usuarioDetalle.EmpleadoId);

            _editEmail = _usuarioDetalle.Email;
            _editActivo = _usuarioDetalle.Activo;
            _editCaducidadDate = _usuarioDetalle.CaducidadContrasena?.LocalDateTime;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _errorDetalle = "Usuario no encontrado.";
        }
        catch
        {
            _errorDetalle = "No se pudo cargar la información del usuario.";
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
        _esNuevo = true;
        _nuevoEmail = string.Empty;
        _nuevoEmpleadoId = null;
        _rolesNuevo.Clear();
        _errorCrear = null;
        _enfocarNuevo = true;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        return Task.CompletedTask;
    }

    protected override async Task EnfocarPrimerCampoNuevoAsync()
        => await _refNuevoEmail.FocusAsync();

    protected override Task OnGuardar()
    {
        if (_esNuevo) return CrearUsuarioAsync();
        return (Tab ?? "datos") switch
        {
            "datos"    => GuardarDatosAsync(),
            "roles"    => GuardarRolesAsync(),
            "empresas" => GuardarEmpresasAsync(),
            _          => Task.CompletedTask
        };
    }

    protected override Task OnBorrar()
    {
        if (!Id.HasValue || _usuarioDetalle is null) return Task.CompletedTask;
        AbrirModalEliminar();
        return Task.CompletedTask;
    }

    // ── Carga auxiliar ─────────────────────────────────────────
    private async Task<IEnumerable<EmpleadoResponse>> BuscarEmpleadosAsync(string query, CancellationToken ct)
    {
        var resultado = await EmpleadosClient.EmpleadosGETAsync(new EmpleadoFilter
        {
            Pagina    = 0,
            PorPagina = 50,
            Busqueda  = string.IsNullOrWhiteSpace(query) ? null : query
        }, ct);
        return resultado.Items;
    }

    private async Task CargarTodosRolesAsync()
    {
        try
        {
            var resultado = await RolesClient.RolesGETAsync(new PaginacionFilter { Pagina = 0, PorPagina = 200 });
            _todosRoles = resultado.Items.ToList();
        }
        catch { /* sin roles disponibles */ }
    }

    private async Task CargarTodasEmpresasAsync()
    {
        try
        {
            var resultado = await EmpresasClient.EmpresasGETAsync(new PaginacionFilter { Pagina = 0, PorPagina = 200 });
            _todasEmpresas = resultado.Items.ToList();
        }
        catch { /* sin empresas disponibles */ }
    }

    // ── Helpers ────────────────────────────────────────────────
    private void LimpiarDetalle()
    {
        _usuarioDetalle = null;
        _empleadoDetalle = null;
        _asignaciones.Clear();
        _empresasUsuario.Clear();
        _addRolStr = "";
        _addContexto = "";
        LimpiarFeedbackDetalle();
    }

    private void LimpiarFeedbackDetalle()
    {
        _errorDatos = null;
        _errorRoles = null;
        _errorReset = null;
        _errorEmpresas = null;
    }

    private void ToggleRolNuevo(int rolId, bool valor)
    {
        if (valor) _rolesNuevo.Add(rolId);
        else _rolesNuevo.Remove(rolId);
    }

    private void ToggleEmpresa(int empresaId, bool valor)
    {
        if (valor) _empresasUsuario.Add(empresaId);
        else _empresasUsuario.Remove(empresaId);
    }

    // ── Acciones ───────────────────────────────────────────────
    private async Task GuardarDatosAsync()
    {
        if (_usuarioDetalle is null || !Id.HasValue) return;

        _errorDatos = null;
        _guardandoDatos = true;
        try
        {
            await UsuariosClient.UsuariosPUTAsync(Id.Value, new UpdateUsuarioRequest
            {
                Email = _editEmail,
                Activo = _editActivo,
                CaducidadContrasena = _editCaducidadDate?.ToUniversalTime()
            });

            Toast.Exito("Datos guardados correctamente.");
            _usuarioDetalle = await UsuariosClient.UsuariosGET2Async(Id.Value);
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            _errorDatos = "El email ya está en uso o los datos no son válidos.";
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

    private async Task GuardarEmpresasAsync()
    {
        if (!Id.HasValue) return;

        _errorEmpresas = null;
        _guardandoEmpresas = true;
        try
        {
            await UsuariosClient.EmpresasAsync(Id.Value, new AsignarEmpresasRequest
            {
                EmpresaIds = _empresasUsuario.ToList()
            });

            Toast.Exito("Empresas actualizadas correctamente.");
        }
        catch
        {
            _errorEmpresas = "No se pudieron actualizar las empresas.";
        }
        finally
        {
            _guardandoEmpresas = false;
        }
    }

    private async Task AñadirAsignacion()
    {
        if (AddRolId is null) return;
        var empresaId = AddEmpresaId;
        if (_asignaciones.Any(a => a.RolId == AddRolId && a.EmpresaId == empresaId)) return;

        _asignaciones.Add(new AsignacionRol { RolId = AddRolId.Value, EmpresaId = empresaId });
        _addRolStr = "";
        await _refAddRol.FocusAsync();
    }

    private void EliminarAsignacion(int rolId, int? empresaId)
        => _asignaciones.RemoveAll(a => a.RolId == rolId && a.EmpresaId == empresaId);

    private async Task GuardarRolesAsync()
    {
        if (!Id.HasValue) return;

        _errorRoles = null;
        _guardandoRoles = true;
        try
        {
            var response = await Http.PutAsJsonAsync(
                $"api/usuarios/{Id.Value}/roles/todas",
                _asignaciones.Select(a => new { a.RolId, a.EmpresaId }).ToList());

            if (!response.IsSuccessStatusCode)
            {
                _errorRoles = "No se pudieron actualizar los roles.";
                return;
            }
            Toast.Exito("Roles actualizados correctamente.");
        }
        catch
        {
            _errorRoles = "No se pudieron actualizar los roles.";
        }
        finally
        {
            _guardandoRoles = false;
        }
    }

    private async Task ResetearContrasenaAsync()
    {
        if (!Id.HasValue) return;

        _errorReset = null;
        _reseteandoContrasena = true;
        try
        {
            await UsuariosClient.ResetearContrasenaAsync(Id.Value);
            Toast.Exito("Contraseña reseteada. Se ha enviado la contraseña temporal por email.");

            _usuarioDetalle = await UsuariosClient.UsuariosGET2Async(Id.Value);
            _editActivo = _usuarioDetalle.Activo;
            _editEmail = _usuarioDetalle.Email;
            _editCaducidadDate = _usuarioDetalle.CaducidadContrasena?.LocalDateTime;
        }
        catch
        {
            _errorReset = "No se pudo resetear la contraseña.";
        }
        finally
        {
            _reseteandoContrasena = false;
        }
    }

    private async Task EliminarUsuarioAsync()
    {
        if (!Id.HasValue) return;

        _errorEliminar = null;
        _eliminando = true;
        try
        {
            await UsuariosClient.UsuariosDELETEAsync(Id.Value);
            _mostrarModalEliminar = false;
            await CargarListaAsync();
            Toast.Exito("Usuario eliminado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        }
        catch
        {
            _errorEliminar = "No se pudo eliminar el usuario.";
        }
        finally
        {
            _eliminando = false;
        }
    }

    private async Task CrearUsuarioAsync()
    {
        _errorCrear = null;

        if (string.IsNullOrWhiteSpace(_nuevoEmail))
        {
            _errorCrear = "El email es obligatorio."; return;
        }
        if (!_nuevoEmpleadoId.HasValue || _nuevoEmpleadoId <= 0)
        {
            _errorCrear = "Selecciona un empleado de la lista."; return;
        }

        _creando = true;
        try
        {
            var creado = await UsuariosClient.UsuariosPOSTAsync(new CreateUsuarioRequest
            {
                Email = _nuevoEmail,
                EmpleadoId = _nuevoEmpleadoId.Value
            });

            if (_rolesNuevo.Count > 0)
            {
                await UsuariosClient.Roles2Async(creado.Id, null, new AsignarRolesRequest
                {
                    RolIds = _rolesNuevo.ToList()
                });
            }

            await CargarListaAsync();
            Toast.Exito("Usuario creado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", creado.Id));
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            _errorCrear = "El email ya está en uso o los datos no son válidos.";
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _errorCrear = "No existe un empleado con ese ID.";
        }
        catch
        {
            _errorCrear = "No se pudo crear el usuario.";
        }
        finally
        {
            _creando = false;
        }
    }

    // ── Modelos locales ────────────────────────────────────────
    private record AsignacionRol
    {
        public int RolId { get; init; }
        public int? EmpresaId { get; init; }
    }

    // ── Inicialización desde fuente externa ────────────────────
    protected override Task InicializarCreacion(JsonElement datos)
    {
        var req = datos.Deserialize<CreateUsuarioRequest>()!;

        _nuevoEmail      = req.Email ?? "";
        _nuevoEmpleadoId = req.EmpleadoId > 0 ? req.EmpleadoId : null;

        return Task.CompletedTask;
    }
}
