using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Components.Common;
using ERPNet.Web.Blazor.Client.Components.Pages.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Rrhh;

public partial class Empleados
{
    [Inject] private IEmpleadosClient EmpleadosClient { get; set; } = default!;
    [Inject] private ISeccionesClient SeccionesClient { get; set; } = default!;

    // ── Lista ──────────────────────────────────────────────────
    private VirtualList<EmpleadoResponse>? _refLista;
    private int? _totalItems;
    private string _ordenarPor = nameof(EmpleadoResponse.Nombre);
    private bool _ordenDesc;
    private List<SeccionResponse> _secciones = [];

    // ── Filtro ─────────────────────────────────────────────────
    private FilterState<EmpleadoFilter> _filtro = new();

    private int _seccionIdEdit
    {
        get => _filtro.Editando.SeccionId ?? 0;
        set => _filtro.Editando.SeccionId = value == 0 ? null : value;
    }

    private static readonly (string Valor, string Label)[] _camposOrden =
    [
        (nameof(EmpleadoResponse.Nombre),        "Nombre"),
        (nameof(EmpleadoResponse.Dni),           "DNI"),
        (nameof(EmpleadoResponse.SeccionNombre), "Sección"),
    ];

    // ── Detalle ────────────────────────────────────────────────
    private EmpleadoResponse? _empleadoDetalle;
    private bool _cargandoDetalle;
    private string? _errorDetalle;

    // ── Formulario edición ─────────────────────────────────────
    private string _editNombre = string.Empty;
    private string _editApellidos = string.Empty;
    private string _editDni = string.Empty;
    private int _editSeccionId;
    private int? _editEncargadoId;
    private bool _editActivo;
    private bool _guardando;
    private string? _errorGuardar;

    // ── Foto ───────────────────────────────────────────────────
    private bool _subiendoFoto;
    private bool _eliminandoFoto;
    private bool _mostrarModalEliminarFoto;
    private string? _errorFoto;
    private int _fotoVersion;

    // ── Eliminar empleado ──────────────────────────────────────────
    private bool _eliminando;
    private string? _errorEliminar { get; set; }

    // ── Formulario creación ────────────────────────────────────
    private ElementReference _refNuevoNombre;
    private string _nuevoNombre = string.Empty;
    private string _nuevoApellidos = string.Empty;
    private string _nuevoDni = string.Empty;
    private int _nuevoSeccionId;
    private int? _nuevoEncargadoId;
    private bool _creando;
    private string? _errorCrear;

    // ── Computed ───────────────────────────────────────────────
    private string InputFotoId => $"input-foto-{Id}";

    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await CargarSeccionesAsync();
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        if (_refLista is not null)
            await _refLista.RefreshAsync();
    }

    internal async ValueTask<ItemsProviderResult<EmpleadoResponse>> CargarItemsAsync(ItemsProviderRequest request)
    {
        try
        {
            var resultado = await EmpleadosClient.EmpleadosGETAsync(
                _filtro.AplicadoCon(f =>
                {
                    f.Pagina     = request.StartIndex;
                    f.PorPagina  = request.Count;
                    f.Busqueda   = string.IsNullOrWhiteSpace(_busqueda) ? null : _busqueda;
                    f.OrdenarPor = _ordenarPor;
                    f.OrdenDesc  = _ordenDesc;
                }), request.CancellationToken);
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
            _empleadoDetalle = await EmpleadosClient.EmpleadosGET2Async(id);

            _editNombre      = _empleadoDetalle.Nombre;
            _editApellidos   = _empleadoDetalle.Apellidos;
            _editDni         = _empleadoDetalle.Dni;
            _editSeccionId   = _empleadoDetalle.SeccionId;
            _editEncargadoId = _empleadoDetalle.EncargadoId;
            _editActivo      = _empleadoDetalle.Activo;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _errorDetalle = "Empleado no encontrado.";
        }
        catch
        {
            _errorDetalle = "No se pudo cargar la información del empleado.";
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
        _nuevoApellidos   = string.Empty;
        _nuevoDni         = string.Empty;
        _nuevoSeccionId   = _secciones.FirstOrDefault()?.Id ?? 0;
        _nuevoEncargadoId = null;
        _errorCrear       = null;
        _enfocarNuevo     = true;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        return Task.CompletedTask;
    }

    protected override Task OnGuardar()
    {
        if (_esNuevo) return CrearEmpleadoAsync();
        return GuardarTodoAsync();
    }

    protected override Task OnBorrar()
    {
        if (!Id.HasValue || _empleadoDetalle is null) return Task.CompletedTask;
        AbrirModalEliminar();
        return Task.CompletedTask;
    }

    private ElementReference _refFiltroNombre;

    protected override Task OnFiltro()
    {
        _filtro.Abrir();
        _enfocarFiltro = true;
        return Task.CompletedTask;
    }

    protected override Task OnLimpiarFiltro() => LimpiarFiltroAsync();

    protected override async Task EnfocarPrimerCampoFiltroAsync()
        => await _refFiltroNombre.FocusAsync();

    protected override Task OnEscape()
    {
        if (_filtro.ModalVisible)     { _filtro.Cancelar(); return Task.CompletedTask; }
        if (_mostrarModalEliminarFoto) { _mostrarModalEliminarFoto = false; return Task.CompletedTask; }
        if (_esNuevo)                 { _esNuevo = false; LimpiarDetalle(); return Task.CompletedTask; }
        return base.OnEscape();
    }

    protected override async Task EnfocarPrimerCampoNuevoAsync()
        => await _refNuevoNombre.FocusAsync();

    // ── Carga auxiliar ─────────────────────────────────────────
    private async Task CargarSeccionesAsync()
    {
        try
        {
            var resultado = await SeccionesClient.SeccionesAsync();
            _secciones = resultado.ToList();
        }
        catch { /* sin secciones disponibles */ }
    }

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

    private async Task SetOrdenAsync(string campo)
    {
        _ordenarPor = campo;
        await CargarListaAsync();
    }

    private async Task ToggleOrdenDescAsync()
    {
        _ordenDesc = !_ordenDesc;
        await CargarListaAsync();
    }

    private async Task AplicarFiltroAsync()
    {
        _filtro.Aplicar();
        await CargarListaAsync();
    }

    private async Task LimpiarFiltroAsync()
    {
        _filtro.Limpiar();
        await CargarListaAsync();
    }

    // ── Helpers ────────────────────────────────────────────────
    private void LimpiarDetalle()
    {
        _empleadoDetalle = null;
        LimpiarFeedbackDetalle();
    }

    private void LimpiarFeedbackDetalle()
    {
        _errorGuardar  = null;
        _errorFoto     = null;
        _errorEliminar = null;
    }

    // ── Acciones ───────────────────────────────────────────────
    private async Task GuardarTodoAsync()
    {
        if (_empleadoDetalle is null || !Id.HasValue) return;

        _errorGuardar = null;
        _guardando    = true;
        try
        {
            await EmpleadosClient.EmpleadosPUTAsync(Id.Value, new UpdateEmpleadoRequest
            {
                Nombre      = _editNombre,
                Apellidos   = _editApellidos,
                Dni         = _editDni,
                SeccionId   = _editSeccionId,
                EncargadoId = _editEncargadoId,
                Activo      = _editActivo
            });

            Toast.Exito("Cambios guardados correctamente.");
            _empleadoDetalle = await EmpleadosClient.EmpleadosGET2Async(Id.Value);
            await CargarListaAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorGuardar = "Ya existe un empleado con ese DNI.";
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            _errorGuardar = "Los datos no son válidos.";
        }
        catch
        {
            _errorGuardar = "No se pudieron guardar los cambios.";
        }
        finally
        {
            _guardando = false;
        }
    }

    private async Task SubirFotoAsync(InputFileChangeEventArgs e)
    {
        if (!Id.HasValue) return;

        _errorFoto    = null;
        _subiendoFoto = true;
        try
        {
            var file = e.File;
            using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            await EmpleadosClient.ArchivosPUTAsync(Id.Value, "foto",
                new FileParameter(stream, file.Name, file.ContentType));

            _fotoVersion++;
            Toast.Exito("Foto actualizada correctamente.");
            _empleadoDetalle = await EmpleadosClient.EmpleadosGET2Async(Id.Value);
            await CargarListaAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            _errorFoto = "Formato no permitido. Usa JPG, PNG o WEBP.";
        }
        catch (Exception ex) when (ex.Message.Contains("exceeds the maximum"))
        {
            _errorFoto = "El archivo supera el límite de 5 MB.";
        }
        catch
        {
            _errorFoto = "No se pudo subir la foto.";
        }
        finally
        {
            _subiendoFoto = false;
        }
    }

    private async Task EliminarFotoAsync()
    {
        if (!Id.HasValue) return;

        _eliminandoFoto = true;
        try
        {
            await EmpleadosClient.ArchivosDELETEAsync(Id.Value, "foto");
            _mostrarModalEliminarFoto = false;
            _fotoVersion++;
            Toast.Exito("Foto eliminada.");
            _empleadoDetalle = await EmpleadosClient.EmpleadosGET2Async(Id.Value);
            await CargarListaAsync();
        }
        catch
        {
            _errorFoto = "No se pudo eliminar la foto.";
            _mostrarModalEliminarFoto = false;
        }
        finally
        {
            _eliminandoFoto = false;
        }
    }

    private async Task EliminarEmpleadoAsync()
    {
        if (!Id.HasValue) return;

        _errorEliminar = null;
        _eliminando    = true;
        try
        {
            await EmpleadosClient.EmpleadosDELETEAsync(Id.Value);
            _mostrarModalEliminar = false;
            await CargarListaAsync();
            Toast.Exito("Empleado eliminado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        }
        catch
        {
            _errorEliminar = "No se pudo eliminar el empleado.";
        }
        finally
        {
            _eliminando = false;
        }
    }

    private async Task CrearEmpleadoAsync()
    {
        _errorCrear = null;

        if (string.IsNullOrWhiteSpace(_nuevoNombre))
        {
            _errorCrear = "El nombre es obligatorio."; return;
        }
        if (string.IsNullOrWhiteSpace(_nuevoApellidos))
        {
            _errorCrear = "Los apellidos son obligatorios."; return;
        }
        if (string.IsNullOrWhiteSpace(_nuevoDni))
        {
            _errorCrear = "El DNI es obligatorio."; return;
        }
        if (_nuevoSeccionId <= 0)
        {
            _errorCrear = "Selecciona una sección."; return;
        }

        _creando = true;
        try
        {
            var creado = await EmpleadosClient.EmpleadosPOSTAsync(new CreateEmpleadoRequest
            {
                Nombre      = _nuevoNombre,
                Apellidos   = _nuevoApellidos,
                Dni         = _nuevoDni,
                SeccionId   = _nuevoSeccionId,
                EncargadoId = _nuevoEncargadoId
            });

            await CargarListaAsync();
            Toast.Exito("Empleado creado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", creado.Id));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorCrear = "Ya existe un empleado con ese DNI.";
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            _errorCrear = "Los datos no son válidos.";
        }
        catch
        {
            _errorCrear = "No se pudo crear el empleado.";
        }
        finally
        {
            _creando = false;
        }
    }
}
