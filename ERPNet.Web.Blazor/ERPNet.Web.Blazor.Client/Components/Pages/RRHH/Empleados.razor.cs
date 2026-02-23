using ERPNet.ApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace ERPNet.Web.Blazor.Client.Components.Pages.RRHH;

public partial class Empleados
{
    [Inject] private IEmpleadosClient EmpleadosClient { get; set; } = default!;
    [Inject] private ISeccionesClient SeccionesClient { get; set; } = default!;

    // ── Paginación ─────────────────────────────────────────────
    protected override int? TotalPaginas => _paginado?.TotalPaginas;

    // ── Lista ──────────────────────────────────────────────────
    private ListaPaginadaOfEmpleadoResponse? _paginado;
    private List<EmpleadoResponse> _empleados = [];
    private List<SeccionResponse> _secciones = [];
    private bool _cargandoLista = true;

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
    private string? _errorEliminar;

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

    private List<EmpleadoResponse> _empleadosFiltrados =>
        string.IsNullOrWhiteSpace(_busqueda)
            ? _empleados
            : _empleados.Where(e =>
                TextHelper.ContieneBusqueda($"{e.Nombre} {e.Apellidos}", _busqueda) ||
                TextHelper.ContieneBusqueda(e.Dni, _busqueda)).ToList();

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
        await Task.WhenAll(CargarListaAsync(), CargarSeccionesAsync());
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        _cargandoLista = true;
        try
        {
            _paginado = await EmpleadosClient.EmpleadosGETAsync(_pagina, PorPagina);
            _empleados = _paginado.Items.ToList();
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
        var resultado = await EmpleadosClient.EmpleadosGETAsync(1, 100, ct);
        return resultado.Items.Where(e =>
            TextHelper.ContieneBusqueda($"{e.Nombre} {e.Apellidos}", query) ||
            TextHelper.ContieneBusqueda(e.Dni, query));
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

    private void ActualizarEnLista(EmpleadoResponse empleado)
    {
        var idx = _empleados.FindIndex(e => e.Id == empleado.Id);
        if (idx >= 0) _empleados[idx] = empleado;
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
            ActualizarEnLista(_empleadoDetalle);
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
            ActualizarEnLista(_empleadoDetalle);
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
            ActualizarEnLista(_empleadoDetalle);
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
