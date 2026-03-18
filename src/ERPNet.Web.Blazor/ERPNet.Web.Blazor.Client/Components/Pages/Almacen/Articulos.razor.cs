using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Components.Common;
using ERPNet.Web.Blazor.Client.Components.Common.Tabs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Almacen;

public partial class Articulos
{
    [Inject] private IArticulosClient ArticulosClient { get; set; } = default!;

    // ── Lista ──────────────────────────────────────────────────
    private VirtualList<ArticuloResponse>? _refLista;
    private int? _totalItems;

    // ── Tabs ───────────────────────────────────────────────────
    private static readonly TabItem[] _tabsArticulo =
    [
        new("datos", "Datos",     "bi-tag"),
        new("log",   "Log",       "bi-journal-text"),
    ];

    // ── Catálogos ──────────────────────────────────────────────
    private List<FamiliaArticuloResponse> _familias = [];
    private List<TipoIvaResponse> _tiposIva = [];
    private List<FormatoArticuloResponse> _formatos = [];

    // ── Estado ─────────────────────────────────────────────────
    private bool _eliminando;
    private string? _errorEliminar { get; set; }

    // ── Detalle ────────────────────────────────────────────────
    private ArticuloResponse? _articuloDetalle;
    private bool _cargandoDetalle;
    private string? _errorDetalle;

    // ── Tab Datos ──────────────────────────────────────────────
    private string _editCodigo = string.Empty;
    private string _editDescripcion = string.Empty;
    private string? _editUnidadMedida;
    private double? _editPrecioCompra;
    private double? _editPrecioVenta;
    private bool _editActivo;
    private int _editFamiliaId;
    private int _editTipoIvaId;
    private int _editFormatoId;
    private bool _guardando;
    private string? _errorGuardar;

    // ── Tab Log ────────────────────────────────────────────────
    private List<ArticuloLogResponse> _logs = [];
    private bool _cargandoLogs;
    private DateTimeOffset _nuevoLogFecha = DateTimeOffset.Now;
    private string _nuevoLogNota = string.Empty;
    private double? _nuevoLogStockAnterior;
    private double? _nuevoLogStockNuevo;
    private bool _guardandoLog;
    private string? _errorLog;

    // ── Formulario creación ────────────────────────────────────
    private ElementReference _refNuevoCodigo;
    private string _nuevoCodigo = string.Empty;
    private string _nuevDescripcion = string.Empty;
    private string? _nuevUnidadMedida;
    private int _nuevFamiliaId;
    private int _nuevTipoIvaId;
    private bool _creando;
    private string? _errorCrear;

    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Task.WhenAll(CargarFamiliasAsync(), CargarTiposIvaAsync(), CargarFormatosAsync());
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        if (_refLista is not null)
            await _refLista.RefreshAsync();
    }

    internal async ValueTask<ItemsProviderResult<ArticuloResponse>> CargarItemsAsync(ItemsProviderRequest request)
    {
        try
        {
            var resultado = await ArticulosClient.ArticulosGETAsync(
                pagina:     request.StartIndex,
                porPagina:  request.Count,
                busqueda:   string.IsNullOrWhiteSpace(_busqueda) ? null : _busqueda,
                ordenarPor: null,
                ordenDesc:  null,
                cancellationToken: request.CancellationToken);
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
        _errorDetalle    = null;
        LimpiarFeedbackDetalle();
        _logs.Clear();

        try
        {
            _articuloDetalle = await ArticulosClient.ArticulosGET2Async(id);

            _editCodigo        = _articuloDetalle.Codigo;
            _editDescripcion   = _articuloDetalle.Descripcion;
            _editUnidadMedida  = _articuloDetalle.UnidadMedida;
            _editPrecioCompra  = _articuloDetalle.PrecioCompra;
            _editPrecioVenta   = _articuloDetalle.PrecioVenta;
            _editActivo        = _articuloDetalle.Activo;
            _editFamiliaId     = _articuloDetalle.FamiliaArticuloId ?? 0;
            _editTipoIvaId     = _articuloDetalle.TipoIvaId ?? 0;
            _editFormatoId     = _articuloDetalle.FormatoArticuloId ?? 0;

            if ((Tab ?? "datos") == "log")
                await CargarLogsAsync(id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _errorDetalle = "Artículo no encontrado.";
        }
        catch
        {
            _errorDetalle = "No se pudo cargar la información del artículo.";
        }
        finally
        {
            _cargandoDetalle = false;
        }
    }

    protected override Task LimpiarDetalleAsync()
    {
        _articuloDetalle = null;
        _logs.Clear();
        LimpiarFeedbackDetalle();
        return Task.CompletedTask;
    }

    protected override Task OnNuevo()
    {
        _esNuevo          = true;
        _nuevoCodigo      = string.Empty;
        _nuevDescripcion  = string.Empty;
        _nuevUnidadMedida = null;
        _nuevFamiliaId    = 0;
        _nuevTipoIvaId    = 0;
        _errorCrear       = null;
        _enfocarNuevo     = true;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        return Task.CompletedTask;
    }

    protected override async Task EnfocarPrimerCampoNuevoAsync()
        => await _refNuevoCodigo.FocusAsync();

    protected override Task OnGuardar()
    {
        if (_esNuevo) return CrearArticuloAsync();
        return (Tab ?? "datos") switch
        {
            "datos" => GuardarDatosAsync(),
            "log"   => GuardarLogAsync(),
            _       => Task.CompletedTask
        };
    }

    protected override Task OnBorrar()
    {
        if (!Id.HasValue || _articuloDetalle is null) return Task.CompletedTask;
        AbrirModalEliminar();
        return Task.CompletedTask;
    }

    // ── Carga auxiliar ─────────────────────────────────────────
    private async Task CargarFamiliasAsync()
    {
        try
        {
            var familias = await ArticulosClient.FamiliasAsync();
            _familias = familias.ToList();
        }
        catch { /* sin familias */ }
    }

    private async Task CargarTiposIvaAsync()
    {
        try
        {
            var tipos = await ArticulosClient.TiposIvaAsync();
            _tiposIva = tipos.ToList();
        }
        catch { /* sin tipos IVA */ }
    }

    private async Task CargarFormatosAsync()
    {
        try
        {
            var formatos = await ArticulosClient.FormatosAsync();
            _formatos = formatos.ToList();
        }
        catch { /* sin formatos */ }
    }

    private async Task CargarLogsAsync(int articuloId)
    {
        _cargandoLogs = true;
        try
        {
            var logs = await ArticulosClient.LogsAllAsync(articuloId);
            _logs = logs.ToList();
        }
        catch { /* sin logs */ }
        finally
        {
            _cargandoLogs = false;
        }
    }

    // ── Helpers ────────────────────────────────────────────────
    private void LimpiarFeedbackDetalle()
    {
        _errorGuardar = null;
        _errorLog     = null;
    }

    // ── Acciones ───────────────────────────────────────────────
    private async Task CrearArticuloAsync()
    {
        _errorCrear = null;

        if (string.IsNullOrWhiteSpace(_nuevoCodigo))
        {
            _errorCrear = "El código es obligatorio.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_nuevDescripcion))
        {
            _errorCrear = "La descripción es obligatoria.";
            return;
        }

        _creando = true;
        try
        {
            var creado = await ArticulosClient.ArticulosPOSTAsync(new CreateArticuloRequest
            {
                Codigo             = _nuevoCodigo,
                Descripcion        = _nuevDescripcion,
                UnidadMedida       = string.IsNullOrWhiteSpace(_nuevUnidadMedida) ? null : _nuevUnidadMedida,
                FamiliaArticuloId  = _nuevFamiliaId == 0 ? null : _nuevFamiliaId,
                TipoIvaId          = _nuevTipoIvaId == 0 ? null : _nuevTipoIvaId,
            });

            await CargarListaAsync();
            Toast.Exito("Artículo creado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", creado.Id));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorCrear = "Ya existe un artículo con ese código.";
        }
        catch
        {
            _errorCrear = "No se pudo crear el artículo.";
        }
        finally
        {
            _creando = false;
        }
    }

    private async Task GuardarDatosAsync()
    {
        if (!Id.HasValue || _articuloDetalle is null) return;

        _errorGuardar = null;
        _guardando    = true;
        try
        {
            await ArticulosClient.ArticulosPUTAsync(Id.Value, new UpdateArticuloRequest
            {
                Codigo             = _editCodigo,
                Descripcion        = _editDescripcion,
                UnidadMedida       = string.IsNullOrWhiteSpace(_editUnidadMedida) ? null : _editUnidadMedida,
                PrecioCompra       = _editPrecioCompra,
                PrecioVenta        = _editPrecioVenta,
                Activo             = _editActivo,
                FamiliaArticuloId  = _editFamiliaId == 0 ? null : _editFamiliaId,
                TipoIvaId          = _editTipoIvaId == 0 ? null : _editTipoIvaId,
                FormatoArticuloId  = _editFormatoId == 0 ? null : _editFormatoId,
            });

            Toast.Exito("Cambios guardados correctamente.");
            _articuloDetalle = await ArticulosClient.ArticulosGET2Async(Id.Value);
            await CargarListaAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorGuardar = "Ya existe un artículo con ese código.";
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

    private async Task GuardarLogAsync()
    {
        if (!Id.HasValue) return;

        _errorLog = null;

        if (string.IsNullOrWhiteSpace(_nuevoLogNota))
        {
            _errorLog = "La nota es obligatoria.";
            return;
        }

        _guardandoLog = true;
        try
        {
            var log = await ArticulosClient.LogsAsync(Id.Value, new CreateArticuloLogRequest
            {
                Fecha         = _nuevoLogFecha,
                Nota          = _nuevoLogNota,
                StockAnterior = _nuevoLogStockAnterior,
                StockNuevo    = _nuevoLogStockNuevo,
            });

            _logs.Insert(0, log);
            _nuevoLogNota          = string.Empty;
            _nuevoLogStockAnterior = null;
            _nuevoLogStockNuevo    = null;
            _nuevoLogFecha         = DateTimeOffset.Now;
            Toast.Exito("Entrada de log añadida.");
        }
        catch
        {
            _errorLog = "No se pudo guardar la entrada de log.";
        }
        finally
        {
            _guardandoLog = false;
        }
    }

    private async Task EliminarArticuloAsync()
    {
        if (!Id.HasValue) return;

        _errorEliminar = null;
        _eliminando    = true;
        try
        {
            await ArticulosClient.ArticulosDELETEAsync(Id.Value);
            _mostrarModalEliminar = false;
            await CargarListaAsync();
            Toast.Exito("Artículo eliminado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        }
        catch
        {
            _errorEliminar = "No se pudo eliminar el artículo.";
        }
        finally
        {
            _eliminando = false;
        }
    }
}
