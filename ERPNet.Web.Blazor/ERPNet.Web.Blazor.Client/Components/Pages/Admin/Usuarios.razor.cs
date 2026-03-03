using ERPNet.ApiClient;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Admin;

public partial class Usuarios
{
    [Inject] private IUsuariosClient UsuariosClient { get; set; } = default!;
    [Inject] private IEmpleadosClient EmpleadosClient { get; set; } = default!;
    [Inject] private IRolesClient RolesClient { get; set; } = default!;
    [Inject] private IEmpresasClient EmpresasClient { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    // ── Paginación ─────────────────────────────────────────────
    protected override int? TotalPaginas => _paginado?.TotalPaginas;

    // ── Lista ──────────────────────────────────────────────────
    private ListaPaginadaOfUsuarioResponse? _paginado;
    private List<UsuarioResponse> _usuarios = [];
    private Dictionary<int, EmpleadoResponse> _empleadosPorId = [];
    private bool _cargandoLista = true;

    // ── Roles (todos) ──────────────────────────────────────────
    private List<RolResponse> _todosRoles = [];

    // ── Empresas (todas) ───────────────────────────────────────
    private List<EmpresaResponse> _todasEmpresas = [];

    // ── Estado de selección ────────────────────────────────────
    private string _tabActiva = "datos";

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
    private string? _errorEliminar;

    // ── Formulario creación ────────────────────────────────────
    private ElementReference _refNuevoEmail;
    private string _nuevoEmail = string.Empty;
    private int? _nuevoEmpleadoId;
    private HashSet<int> _rolesNuevo = [];
    private bool _creando;
    private string? _errorCrear;

    // ── Computed ───────────────────────────────────────────────
    private List<UsuarioResponse> _usuariosFiltrados =>
        string.IsNullOrWhiteSpace(_busqueda)
            ? _usuarios
            : _usuarios.Where(u => u.Email.Contains(_busqueda, StringComparison.OrdinalIgnoreCase)).ToList();

    private string PaginacionTexto
    {
        get
        {
            if (_paginado is null) return string.Empty;
            var desde = (_pagina - 1) * PorPagina + 1;
            var hasta = Math.Min(_pagina * PorPagina, _paginado.TotalRegistros);
            return $"{desde}–{hasta} de {_paginado.TotalRegistros}";
        }
    }

    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(CargarListaAsync(), CargarTodosRolesAsync(), CargarTodasEmpresasAsync());
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        _cargandoLista = true;
        try
        {
            _paginado = await UsuariosClient.UsuariosGETAsync(_pagina, PorPagina);
            _usuarios = _paginado.Items.ToList();

            var ids = _usuarios.Select(u => u.EmpleadoId).Distinct().ToList();
            var empleados = await Task.WhenAll(ids.Select(async id =>
            {
                try { return await EmpleadosClient.EmpleadosGET2Async(id); }
                catch { return null; }
            }));
            _empleadosPorId = empleados.OfType<EmpleadoResponse>().ToDictionary(e => e.Id);
        }
        catch { /* lista queda vacía */ }
        finally
        {
            _cargandoLista = false;
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
        return _tabActiva switch
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
        var resultado = await EmpleadosClient.EmpleadosGETAsync(1, 100, ct);
        return resultado.Items.Where(e =>
            TextHelper.ContieneBusqueda($"{e.Nombre} {e.Apellidos}", query) ||
            TextHelper.ContieneBusqueda(e.Dni, query));
    }

    private async Task CargarTodosRolesAsync()
    {
        try
        {
            var resultado = await RolesClient.RolesGETAsync(1, 200);
            _todosRoles = resultado.Items.ToList();
        }
        catch { /* sin roles disponibles */ }
    }

    private async Task CargarTodasEmpresasAsync()
    {
        try
        {
            var resultado = await EmpresasClient.EmpresasGETAsync(1, 200);
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
            var idx = _usuarios.FindIndex(u => u.Id == Id.Value);
            if (idx >= 0) _usuarios[idx] = _usuarioDetalle;
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
            var idx = _usuarios.FindIndex(u => u.Id == Id.Value);
            if (idx >= 0) _usuarios[idx] = _usuarioDetalle;
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

}
