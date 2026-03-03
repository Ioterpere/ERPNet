using ERPNet.ApiClient;
using Microsoft.AspNetCore.Components;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Admin;

public partial class Empresas
{
    [Inject] private IEmpresasClient EmpresasClient { get; set; } = default!;

    // ── Paginación ─────────────────────────────────────────────
    protected override int? TotalPaginas => _paginado?.TotalPaginas;

    // ── Lista ──────────────────────────────────────────────────
    private ListaPaginadaOfEmpresaResponse? _paginado;
    private List<EmpresaResponse> _empresas = [];
    private bool _cargandoLista = true;

    // ── Detalle ────────────────────────────────────────────────
    private EmpresaResponse? _empresaDetalle;
    private bool _cargandoDetalle;
    private string? _errorDetalle;

    // ── Formulario edición ─────────────────────────────────────
    private string _editNombre = string.Empty;
    private string? _editCif;
    private bool _editActivo;
    private bool _guardando;
    private string? _errorGuardar;

    // ── Formulario creación ────────────────────────────────────
    private ElementReference _refNuevoNombre;
    private string _nuevoNombre = string.Empty;
    private string? _nuevoCif;
    private bool _nuevoActivo = true;
    private bool _creando;
    private string? _errorCrear;

    // ── Eliminar ───────────────────────────────────────────────
    private bool _eliminando;
    private string? _errorEliminar;

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
        await CargarListaAsync();
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        _cargandoLista = true;
        try
        {
            _paginado = await EmpresasClient.EmpresasGETAsync(_pagina, PorPagina, string.IsNullOrWhiteSpace(_busqueda) ? null : _busqueda);
            _empresas = _paginado?.Items?.ToList() ?? [];
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
        _errorGuardar = null;

        try
        {
            _empresaDetalle = await EmpresasClient.EmpresasGET2Async(id);

            if (_empresaDetalle is not null)
            {
                _editNombre = _empresaDetalle.Nombre;
                _editCif    = _empresaDetalle.Cif;
                _editActivo = _empresaDetalle.Activo;
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _errorDetalle = "Empresa no encontrada.";
        }
        catch
        {
            _errorDetalle = "No se pudo cargar la información de la empresa.";
        }
        finally
        {
            _cargandoDetalle = false;
        }
    }

    protected override Task LimpiarDetalleAsync()
    {
        _empresaDetalle = null;
        _errorDetalle = null;
        _errorGuardar = null;
        return Task.CompletedTask;
    }

    protected override Task OnNuevo()
    {
        _esNuevo    = true;
        _nuevoNombre = string.Empty;
        _nuevoCif   = null;
        _nuevoActivo = true;
        _errorCrear  = null;
        _enfocarNuevo = true;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        return Task.CompletedTask;
    }

    protected override async Task EnfocarPrimerCampoNuevoAsync()
        => await _refNuevoNombre.FocusAsync();

    protected override Task OnGuardar()
    {
        if (_esNuevo) return CrearEmpresaAsync();
        return GuardarDatosAsync();
    }

    protected override Task OnBorrar()
    {
        if (!Id.HasValue || _empresaDetalle is null) return Task.CompletedTask;
        AbrirModalEliminar();
        return Task.CompletedTask;
    }

    // ── Acciones ───────────────────────────────────────────────
    private async Task CrearEmpresaAsync()
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
            var creada = await EmpresasClient.EmpresasPOSTAsync(new CreateEmpresaRequest
            {
                Nombre = _nuevoNombre,
                Cif    = string.IsNullOrWhiteSpace(_nuevoCif) ? null : _nuevoCif,
                Activo = _nuevoActivo
            });

            await CargarListaAsync();
            Toast.Exito("Empresa creada correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", creada.Id));
        }
        catch
        {
            _errorCrear = "No se pudo crear la empresa.";
        }
        finally
        {
            _creando = false;
        }
    }

    private async Task GuardarDatosAsync()
    {
        if (_empresaDetalle is null || !Id.HasValue) return;

        _errorGuardar = null;
        _guardando = true;
        try
        {
            await EmpresasClient.EmpresasPUTAsync(Id.Value, new UpdateEmpresaRequest
            {
                Nombre = _editNombre,
                Cif    = string.IsNullOrWhiteSpace(_editCif) ? null : _editCif,
                Activo = _editActivo
            });

            _empresaDetalle.Nombre = _editNombre;
            _empresaDetalle.Cif    = string.IsNullOrWhiteSpace(_editCif) ? null : _editCif;
            _empresaDetalle.Activo = _editActivo;

            var idx = _empresas.FindIndex(e => e.Id == Id.Value);
            if (idx >= 0) _empresas[idx] = _empresaDetalle;

            Toast.Exito("Datos guardados correctamente.");
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

    private async Task EliminarEmpresaAsync()
    {
        if (!Id.HasValue) return;

        _errorEliminar = null;
        _eliminando    = true;
        try
        {
            await EmpresasClient.EmpresasDELETEAsync(Id.Value);
            _mostrarModalEliminar = false;
            await CargarListaAsync();
            Toast.Exito("Empresa eliminada correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        }
        catch
        {
            _errorEliminar = "No se pudo eliminar la empresa.";
        }
        finally
        {
            _eliminando = false;
        }
    }

}
