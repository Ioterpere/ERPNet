using System.Globalization;
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
    private string _ordenarPor = nameof(ArticuloResponse.Codigo);
    private bool _ordenDesc;

    private static readonly (string Valor, string Label)[] _camposOrden =
    [
        (nameof(ArticuloResponse.Codigo),                "Código"),
        (nameof(ArticuloResponse.Descripcion),           "Descripción"),
        (nameof(ArticuloResponse.FamiliaArticuloNombre), "Familia"),
        (nameof(ArticuloResponse.PrecioCoste),           "Precio coste"),
        (nameof(ArticuloResponse.PrecioVenta),           "Precio venta"),
    ];

    // ── Tabs ───────────────────────────────────────────────────
    private static readonly TabItem[] _tabsArticulo =
    [
        new("datos",       "Datos",           "bi-tag"),
        new("facturacion", "C. Facturación",  "bi-receipt"),
        new("pallet",      "Tpo. Pallet",     "bi-box-seam"),
        new("dpr",         "DPR",             "bi-graph-up"),
        new("movimiento",  "Geo. Movimiento", "bi-geo-alt"),
        new("rappel",      "Tpo. Rappel",     "bi-percent"),
        new("calidad",     "Calidad",         "bi-shield-check"),
        new("log",         "Log",             "bi-journal-text"),
    ];

    // ── Catálogos ──────────────────────────────────────────────
    private List<FamiliaArticuloResponse> _familias = [];
    private List<TipoIvaResponse> _tiposIva = [];
    private List<FormatoArticuloResponse> _formatos = [];
    private List<ConfiguracionCaducidadResponse> _configuracionesCaducidad = [];

    // ── Estado ─────────────────────────────────────────────────
    private bool _eliminando;
    private string? _errorEliminar { get; set; }

    // ── Detalle ────────────────────────────────────────────────
    private ArticuloResponse? _articuloDetalle;
    private bool _cargandoDetalle;
    private string? _errorDetalle;

    // ── Tab Datos: ficha ───────────────────────────────────────
    private string _editCodigo = string.Empty;
    private string? _editCodigoBarras;
    private string _editDescripcion = string.Empty;
    private string? _editDescripcionVenta;
    private string? _editUnidadMedida;
    private string? _editUnidadMedida2;
    private int _editFamiliaId;
    private int _editTipoIvaId;
    private int _editFormatoId;
    private int _editCaducidadId;
    private string? _editProveedorPrincipal;

    // ── Tab Datos: precios ─────────────────────────────────────
    private double _editPrecioCoste;
    private double _editPrecioMedio;
    private double _editPrecioVenta;

    // ── Tab Datos: stock y logística ───────────────────────────
    private double _editStockMinimo;
    private double _editNivelPedido;
    private double _editNivelReposicion;
    private double _editUnidadesCaja;
    private int _editUnidadesPalet;
    private int _editFilasPalet;
    private double _editPesoGramo;
    private int? _editLeadTime;
    private int _editDiasVidaUtil;
    private double _editDepreciacion;

    // ── Tab Datos: flags ───────────────────────────────────────
    private bool _editEsInventariable;
    private bool _editEsPropio;
    private bool _editEsNuevo;
    private bool _editEsObsoleto;
    private string? _editObservaciones;

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
    private bool _creando;
    private string? _errorCrear;

    // ── Formato de precios (es-ES) ─────────────────────────────
    private static readonly CultureInfo CulturaEs = new("es-ES");

    private static string FormatPrecio(double valor) =>
        valor.ToString("N2", CulturaEs);

    private static double ParsePrecio(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return 0;
        if (double.TryParse(texto, NumberStyles.Any, CulturaEs, out var v)) return v;
        if (double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out v)) return v;
        return 0;
    }

    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Task.WhenAll(CargarFamiliasAsync(), CargarTiposIvaAsync(), CargarFormatosAsync(), CargarConfiguracionesCaducidadAsync());
    }

    // ── Implementación de abstracts ────────────────────────────
    protected override async Task CargarListaAsync()
    {
        if (_refLista is not null)
            await _refLista.RefreshAsync();
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

    internal async ValueTask<ItemsProviderResult<ArticuloResponse>> CargarItemsAsync(ItemsProviderRequest request)
    {
        try
        {
            var resultado = await ArticulosClient.ArticulosGETAsync(
                pagina:     request.StartIndex,
                porPagina:  request.Count,
                busqueda:   string.IsNullOrWhiteSpace(_busqueda) ? null : _busqueda,
                ordenarPor: _ordenarPor,
                ordenDesc:  _ordenDesc,
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
            MapearDetalleAEdit(_articuloDetalle);

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

    private async Task CargarConfiguracionesCaducidadAsync()
    {
        try
        {
            var configs = await ArticulosClient.ConfiguracionesCaducidadAsync();
            _configuracionesCaducidad = configs.ToList();
        }
        catch { /* sin configuraciones */ }
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

    private void MapearDetalleAEdit(ArticuloResponse a)
    {
        _editCodigo              = a.Codigo;
        _editCodigoBarras        = a.CodigoBarras;
        _editDescripcion         = a.Descripcion;
        _editDescripcionVenta    = a.DescripcionVenta;
        _editUnidadMedida        = a.UnidadMedida;
        _editUnidadMedida2       = a.UnidadMedida2;
        _editFamiliaId           = a.FamiliaArticuloId ?? 0;
        _editTipoIvaId           = a.TipoIvaId ?? 0;
        _editFormatoId           = a.FormatoArticuloId ?? 0;
        _editCaducidadId         = a.ConfiguracionCaducidadId ?? 0;
        _editProveedorPrincipal  = a.ProveedorPrincipal;
        _editPrecioCoste         = (double)a.PrecioCoste;
        _editPrecioMedio         = (double)a.PrecioMedio;
        _editPrecioVenta         = (double)a.PrecioVenta;
        _editStockMinimo         = (double)a.StockMinimo;
        _editNivelPedido         = (double)a.NivelPedido;
        _editNivelReposicion     = (double)a.NivelReposicion;
        _editUnidadesCaja        = (double)a.UnidadesCaja;
        _editUnidadesPalet       = a.UnidadesPalet;
        _editFilasPalet          = a.FilasPalet;
        _editPesoGramo           = (double)a.PesoGramo;
        _editLeadTime            = a.LeadTime;
        _editDiasVidaUtil        = a.DiasVidaUtil;
        _editDepreciacion        = (double)a.Depreciacion;
        _editEsInventariable     = a.EsInventariable;
        _editEsPropio            = a.EsPropio;
        _editEsNuevo             = a.EsNuevo;
        _editEsObsoleto          = a.EsObsoleto;
        _editObservaciones       = a.Observaciones;
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
                Codigo            = _nuevoCodigo.Trim(),
                Descripcion       = _nuevDescripcion.Trim(),
                UnidadMedida      = string.IsNullOrWhiteSpace(_nuevUnidadMedida) ? null : _nuevUnidadMedida.Trim().ToUpper(),
                FamiliaArticuloId = _nuevFamiliaId == 0 ? null : _nuevFamiliaId,
                EsInventariable   = true,
            });

            await CargarListaAsync();
            Toast.Exito("Artículo creado correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", creado.Id));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorCrear = "Ya existe un artículo con ese código.";
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            _errorCrear = "Los datos no son válidos.";
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
                CodigoBarras       = _editCodigoBarras ?? string.Empty,
                Descripcion        = _editDescripcion,
                DescripcionVenta   = _editDescripcionVenta ?? string.Empty,
                UnidadMedida       = string.IsNullOrWhiteSpace(_editUnidadMedida) ? string.Empty : _editUnidadMedida.ToUpper(),
                UnidadMedida2      = string.IsNullOrWhiteSpace(_editUnidadMedida2) ? string.Empty : _editUnidadMedida2.ToUpper(),
                FamiliaArticuloId        = _editFamiliaId,
                TipoIvaId                = _editTipoIvaId,
                FormatoArticuloId        = _editFormatoId,
                ConfiguracionCaducidadId = _editCaducidadId == 0 ? null : _editCaducidadId,
                ProveedorPrincipal       = _editProveedorPrincipal ?? string.Empty,
                PrecioCoste        = _editPrecioCoste,
                PrecioMedio        = _editPrecioMedio,
                PrecioVenta        = _editPrecioVenta,
                StockMinimo        = _editStockMinimo,
                NivelPedido        = _editNivelPedido,
                NivelReposicion    = _editNivelReposicion,
                UnidadesCaja       = _editUnidadesCaja,
                UnidadesPalet      = _editUnidadesPalet,
                FilasPalet         = _editFilasPalet,
                PesoGramo          = _editPesoGramo,
                LeadTime           = _editLeadTime,
                DiasVidaUtil       = _editDiasVidaUtil,
                Depreciacion       = _editDepreciacion,
                EsInventariable    = _editEsInventariable,
                EsPropio           = _editEsPropio,
                EsNuevo            = _editEsNuevo,
                EsObsoleto         = _editEsObsoleto,
                Observaciones      = _editObservaciones ?? string.Empty,
            });

            Toast.Exito("Cambios guardados correctamente.");
            _articuloDetalle = await ArticulosClient.ArticulosGET2Async(Id.Value);
            await CargarListaAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorGuardar = "Ya existe un artículo con ese código.";
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
