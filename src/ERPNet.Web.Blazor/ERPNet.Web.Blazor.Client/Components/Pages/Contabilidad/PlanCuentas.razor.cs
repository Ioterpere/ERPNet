using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Components.Common;
using ERPNet.Web.Blazor.Client.Components.Common.Tabs;
using ERPNet.Web.Blazor.Client.Components.Pages.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Contabilidad;

public partial class PlanCuentas
{
    [Inject] private IPlanCuentasClient PlanCuentasClient { get; set; } = default!;
    [Inject] private IEmpresasClient EmpresasClient { get; set; } = default!;

    // ── Lista ──────────────────────────────────────────────────
    private VirtualList<CuentaResponse>? _refLista;
    private int? _totalItems;
    private FilterState<CuentaFilter> _filtro = InicializarFiltro();
    private string _ordenarPor = nameof(CuentaResponse.Codigo);
    private bool _ordenDesc;

    private static readonly (string Valor, string Label)[] _camposOrden =
    [
        (nameof(CuentaResponse.Codigo),      "Código"),
        (nameof(CuentaResponse.Descripcion), "Descripción"),
    ];

    private static FilterState<CuentaFilter> InicializarFiltro()
    {
        var f = new FilterState<CuentaFilter>();
        f.Abrir();
        f.Editando.SoloConApuntes = true;
        f.Aplicar();
        return f;
    }

    // ── Tabs ───────────────────────────────────────────────────
    private static readonly TabItem[] _tabsCuenta =
    [
        new("extracto", "Extracto", "bi-table"),
        new("saldos",   "Saldos",   "bi-bar-chart"),
        new("cuenta",   "Cuenta",   "bi-pencil-square"),
    ];

    // ── Catálogos ──────────────────────────────────────────────
    private List<TipoDiarioResponse> _tiposDiario = [];
    private List<CentroCosteResponse> _centros = [];

    // ── Detalle ────────────────────────────────────────────────
    private CuentaResponse? _cuentaDetalle;
    private bool _cargandoDetalle;
    private string? _errorDetalle;

    // ── Edición (tab Cuenta) ──────────────────────────────────
    private string _editDescripcion = string.Empty;
    private string _editDescripcionSII = string.Empty;
    private string _editNif = string.Empty;
    private bool _editEsObsoleta;
    private int? _editCuentaAmortizacionId;
    private int? _editCuentaPagoDelegadoId;
    private int? _editEmpresaVinculadaId;
    private int? _editConceptoAnaliticaId;
    private int? _editClienteAsociadoId;
    private CuentaResponse? _preselCuentaAmortizacion;
    private CuentaResponse? _preselCuentaPagoDelegado;
    private EmpresaResponse? _preselEmpresaVinculada;
    private bool _guardandoDatos;
    private string? _errorGuardar;
    private ElementReference _refEditDescripcion;

    // ── Eliminación ───────────────────────────────────────────
    private bool _eliminando;
    private string? _errorEliminar { get; set; }

    // ── Formulario creación ───────────────────────────────────
    private ElementReference _refNuevoCodigo;
    private string _nuevoCodigo = string.Empty;
    private string _nuevoDescripcion = string.Empty;
    private string _nuevoDescripcionSII = string.Empty;
    private string _nuevoNif = string.Empty;
    private string? _errorCrear;
    private bool _creando;

    // ── Extracto ───────────────────────────────────────────────
    private List<ApunteContableResponse> _apuntes = [];
    private bool _cargandoExtracto;
    private int _filtroDiarioId;
    private int _filtroCentroId;
    private DateOnly? _filtroDesde;
    private DateOnly? _filtroHasta;
    private bool _filtroDefinitivo;
    private bool _filtroSinPuntear;

    private double _saldoPeriodo => _apuntes.Sum(a => a.Debe) - _apuntes.Sum(a => a.Haber);

    // ── Saldos ─────────────────────────────────────────────────
    private List<SaldoMensualResponse> _saldosMensuales = [];
    private bool _cargandoSaldos;
    private int _anioSaldos = DateTime.Now.Year;

    // ── Ciclo de vida ──────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await CargarCatalogosAsync();
    }

    // ── Implementación abstracts ───────────────────────────────
    protected override async Task CargarListaAsync()
    {
        if (_refLista is not null)
            await _refLista.RefreshAsync();
    }

    internal async ValueTask<ItemsProviderResult<CuentaResponse>> CargarItemsAsync(ItemsProviderRequest request)
    {
        try
        {
            DateTimeOffset? desde = _filtroDesde.HasValue
                ? new DateTimeOffset(_filtroDesde.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : null;
            DateTimeOffset? hasta = _filtroHasta.HasValue
                ? new DateTimeOffset(_filtroHasta.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero) : null;

            var resultado = await PlanCuentasClient.PlanCuentasGETAsync(
                codigo:            _filtro.Aplicado.Codigo,
                descripcion:       _filtro.Aplicado.Descripcion,
                nif:               _filtro.Aplicado.Nif,
                conNif:            _filtro.Aplicado.ConNif,
                conDescripcionSii: _filtro.Aplicado.ConDescripcionSii,
                conSaldo:          _filtro.Aplicado.ConSaldo,
                soloConApuntes:    _filtro.Aplicado.SoloConApuntes,
                tipoDiarioId:      _filtroDiarioId > 0 ? _filtroDiarioId : null,
                centroCosteId:     _filtroCentroId > 0 ? _filtroCentroId : null,
                desde:             desde,
                hasta:             hasta,
                esDefinitivo:      _filtroDefinitivo ? true : null,
                punteado:          _filtroSinPuntear ? false : null,
                pagina:            request.StartIndex,
                porPagina:         request.Count,
                busqueda:          string.IsNullOrWhiteSpace(_busqueda) ? null : _busqueda,
                ordenarPor:        _ordenarPor,
                ordenDesc:         _ordenDesc);

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
        _errorGuardar = null;
        _apuntes = [];
        _saldosMensuales = [];

        try
        {
            _cuentaDetalle = await PlanCuentasClient.PlanCuentasGET2Async(id);
            CargarCamposEdicion();
            await CargarTabActivaAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _errorDetalle = "Cuenta no encontrada.";
        }
        catch
        {
            _errorDetalle = "No se pudo cargar la información de la cuenta.";
        }
        finally
        {
            _cargandoDetalle = false;
        }
    }

    protected override Task LimpiarDetalleAsync()
    {
        _cuentaDetalle = null;
        _apuntes = [];
        _saldosMensuales = [];
        _errorDetalle = null;
        _errorGuardar = null;
        _errorEliminar = null;
        return Task.CompletedTask;
    }

    protected override Task OnNuevo()
    {
        _esNuevo              = true;
        _nuevoCodigo          = string.Empty;
        _nuevoDescripcion     = string.Empty;
        _nuevoDescripcionSII  = string.Empty;
        _nuevoNif             = string.Empty;
        _errorCrear           = null;
        _enfocarNuevo         = true;
        Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        return Task.CompletedTask;
    }

    protected override Task OnGuardar()
    {
        if (_esNuevo) return CrearCuentaAsync();
        return GuardarDatosAsync();
    }

    protected override Task OnBorrar()
    {
        if (!Id.HasValue || _cuentaDetalle is null) return Task.CompletedTask;
        AbrirModalEliminar();
        return Task.CompletedTask;
    }

    protected override Task OnEscape()
    {
        if (_esNuevo) { _esNuevo = false; LimpiarDetalle(); return Task.CompletedTask; }
        return base.OnEscape();
    }

    protected override async Task EnfocarPrimerCampoNuevoAsync()
        => await _refNuevoCodigo.FocusAsync();

    // ── Carga tab activa ───────────────────────────────────────
    private async Task CargarTabActivaAsync()
    {
        if (!Id.HasValue) return;
        var tab = Tab ?? "extracto";
        if (tab == "saldos")
            await CargarSaldosAsync();
        else if (tab == "extracto")
            await CargarExtractoAsync();
    }

    // ── Catálogos ──────────────────────────────────────────────
    private async Task CargarCatalogosAsync()
    {
        try
        {
            var tiposTask   = PlanCuentasClient.TiposDiarioAsync();
            var centrosTask = PlanCuentasClient.CentrosCosteAsync();
            await Task.WhenAll(tiposTask, centrosTask);
            _tiposDiario = (await tiposTask).ToList();
            _centros     = (await centrosTask).ToList();
        }
        catch { /* sin catálogos */ }
    }

    // ── Orden lista ────────────────────────────────────────────
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

    // ── Filtro lateral (modal) ─────────────────────────────────
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

    // ── Edición (tab Cuenta) ──────────────────────────────────
    private void CargarCamposEdicion()
    {
        if (_cuentaDetalle is null) return;
        _editDescripcion          = _cuentaDetalle.Descripcion;
        _editDescripcionSII       = _cuentaDetalle.DescripcionSII ?? string.Empty;
        _editNif                  = _cuentaDetalle.Nif ?? string.Empty;
        _editEsObsoleta           = _cuentaDetalle.EsObsoleta;
        _editCuentaAmortizacionId = _cuentaDetalle.CuentaAmortizacionId;
        _editCuentaPagoDelegadoId = _cuentaDetalle.CuentaPagoDelegadoId;
        _editEmpresaVinculadaId   = _cuentaDetalle.EmpresaVinculadaId;
        _editConceptoAnaliticaId  = _cuentaDetalle.ConceptoAnaliticaId;
        _editClienteAsociadoId    = _cuentaDetalle.ClienteAsociadoId;
        _errorGuardar             = null;

        _preselCuentaAmortizacion = _cuentaDetalle.CuentaAmortizacionId.HasValue
            ? new CuentaResponse { Id = _cuentaDetalle.CuentaAmortizacionId.Value,
                Codigo = _cuentaDetalle.CuentaAmortizacionCodigo ?? "", Descripcion = _cuentaDetalle.CuentaAmortizacionDescripcion ?? "" }
            : null;

        _preselCuentaPagoDelegado = _cuentaDetalle.CuentaPagoDelegadoId.HasValue
            ? new CuentaResponse { Id = _cuentaDetalle.CuentaPagoDelegadoId.Value,
                Codigo = _cuentaDetalle.CuentaPagoDelegadoCodigo ?? "", Descripcion = _cuentaDetalle.CuentaPagoDelegadoDescripcion ?? "" }
            : null;

        _preselEmpresaVinculada = _cuentaDetalle.EmpresaVinculadaId.HasValue
            ? new EmpresaResponse { Id = _cuentaDetalle.EmpresaVinculadaId.Value,
                Nombre = _cuentaDetalle.EmpresaVinculadaNombre ?? "" }
            : null;
    }

    private async Task GuardarDatosAsync()
    {
        if (_cuentaDetalle is null || !Id.HasValue) return;

        _errorGuardar   = null;
        _guardandoDatos = true;
        try
        {
            await PlanCuentasClient.PlanCuentasPUTAsync(Id.Value, new UpdateCuentaRequest
            {
                Descripcion          = string.IsNullOrWhiteSpace(_editDescripcion)    ? null : _editDescripcion,
                DescripcionSII       = string.IsNullOrWhiteSpace(_editDescripcionSII) ? null : _editDescripcionSII,
                Nif                  = string.IsNullOrWhiteSpace(_editNif)            ? null : _editNif,
                EsObsoleta           = _editEsObsoleta,
                CuentaAmortizacionId = _editCuentaAmortizacionId ?? 0,
                CuentaPagoDelegadoId = _editCuentaPagoDelegadoId ?? 0,
                EmpresaVinculadaId   = _editEmpresaVinculadaId ?? 0,
                ConceptoAnaliticaId  = _editConceptoAnaliticaId ?? 0,
                ClienteAsociadoId    = _editClienteAsociadoId ?? 0,
            });

            Toast.Exito("Datos guardados correctamente.");
            _cuentaDetalle = await PlanCuentasClient.PlanCuentasGET2Async(Id.Value);
            CargarCamposEdicion();
            await CargarListaAsync();
        }
        catch
        {
            _errorGuardar = "No se pudieron guardar los cambios.";
        }
        finally
        {
            _guardandoDatos = false;
        }
    }

    // ── Eliminación ───────────────────────────────────────────
    private async Task EliminarCuentaAsync()
    {
        if (!Id.HasValue) return;

        _errorEliminar = null;
        _eliminando    = true;
        try
        {
            await PlanCuentasClient.PlanCuentasDELETEAsync(Id.Value);
            _mostrarModalEliminar = false;
            await CargarListaAsync();
            Toast.Exito("Cuenta eliminada correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", (int?)null));
        }
        catch
        {
            _errorEliminar = "No se pudo eliminar la cuenta.";
        }
        finally
        {
            _eliminando = false;
        }
    }

    // ── Creación ──────────────────────────────────────────────
    private async Task CrearCuentaAsync()
    {
        _errorCrear = null;

        if (string.IsNullOrWhiteSpace(_nuevoCodigo))
        {
            _errorCrear = "El código es obligatorio."; return;
        }
        if (string.IsNullOrWhiteSpace(_nuevoDescripcion))
        {
            _errorCrear = "La descripción es obligatoria."; return;
        }

        _creando = true;
        try
        {
            var creada = await PlanCuentasClient.PlanCuentasPOSTAsync(new CreateCuentaRequest
            {
                Codigo         = _nuevoCodigo.Trim(),
                Descripcion    = _nuevoDescripcion.Trim(),
                DescripcionSII = string.IsNullOrWhiteSpace(_nuevoDescripcionSII) ? null : _nuevoDescripcionSII.Trim(),
                Nif            = string.IsNullOrWhiteSpace(_nuevoNif) ? null : _nuevoNif.Trim(),
            });

            await CargarListaAsync();
            Toast.Exito("Cuenta creada correctamente.");
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("id", creada.Id));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            _errorCrear = "Ya existe una cuenta con ese código.";
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            _errorCrear = "Los datos no son válidos.";
        }
        catch
        {
            _errorCrear = "No se pudo crear la cuenta.";
        }
        finally
        {
            _creando = false;
        }
    }

    // ── Helpers ────────────────────────────────────────────────
    private void LimpiarDetalle()
    {
        _cuentaDetalle = null;
        _errorGuardar  = null;
        _errorEliminar = null;
    }

    // ── Extracto ───────────────────────────────────────────────
    private async Task CargarExtractoAsync()
    {
        if (!Id.HasValue) return;

        _cargandoExtracto = true;
        try
        {
            DateTimeOffset? desde = _filtroDesde.HasValue
                ? new DateTimeOffset(_filtroDesde.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : null;
            DateTimeOffset? hasta = _filtroHasta.HasValue
                ? new DateTimeOffset(_filtroHasta.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero) : null;

            var apuntes = await PlanCuentasClient.ExtractoAsync(
                id:            Id.Value,
                tipoDiarioId:  _filtroDiarioId > 0 ? _filtroDiarioId : null,
                centroCosteId: _filtroCentroId > 0 ? _filtroCentroId : null,
                desde:         desde,
                hasta:         hasta,
                esDefinitivo:  _filtroDefinitivo ? true : null,
                punteado:      _filtroSinPuntear ? false : null);

            _apuntes = apuntes.ToList();
        }
        catch
        {
            _apuntes = [];
        }
        finally
        {
            _cargandoExtracto = false;
        }
    }

    private async Task AplicarFiltroExtractoAsync()
    {
        await CargarExtractoAsync();
        await CargarListaAsync();
        await VerificarCuentaActualEnFiltroAsync();
    }

    private async Task VerificarCuentaActualEnFiltroAsync()
    {
        if (!Id.HasValue || _cuentaDetalle is null) return;

        bool hayFiltroActivo = _filtroDiarioId > 0
            || _filtroCentroId > 0
            || _filtroDesde.HasValue
            || _filtroHasta.HasValue
            || _filtroDefinitivo
            || _filtroSinPuntear;

        if (!hayFiltroActivo) return;

        DateTimeOffset? desde = _filtroDesde.HasValue
            ? new DateTimeOffset(_filtroDesde.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : null;
        DateTimeOffset? hasta = _filtroHasta.HasValue
            ? new DateTimeOffset(_filtroHasta.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero) : null;

        try
        {
            var resultado = await PlanCuentasClient.PlanCuentasGETAsync(
                codigo:            _cuentaDetalle.Codigo,
                descripcion:       null,
                nif:               null,
                conNif:            null,
                conDescripcionSii: null,
                conSaldo:          null,
                soloConApuntes:    null,
                tipoDiarioId:      _filtroDiarioId > 0 ? _filtroDiarioId : null,
                centroCosteId:     _filtroCentroId > 0 ? _filtroCentroId : null,
                desde:             desde,
                hasta:             hasta,
                esDefinitivo:      _filtroDefinitivo ? true : null,
                punteado:          _filtroSinPuntear ? false : null,
                pagina:            0,
                porPagina:         1,
                busqueda:          null,
                ordenarPor:        null,
                ordenDesc:         false);

            if (!resultado.Items.Any(c => c.Id == Id.Value))
            {
                Toast.Aviso($"La cuenta «{_cuentaDetalle.Codigo} — {_cuentaDetalle.Descripcion}» no cumple con los criterios seleccionados.");
            }
        }
        catch { /* silencioso */ }
    }

    private async Task LimpiarFiltroExtractoAsync()
    {
        _filtroDiarioId   = 0;
        _filtroCentroId   = 0;
        _filtroDesde      = null;
        _filtroHasta      = null;
        _filtroDefinitivo = false;
        _filtroSinPuntear = false;
        await CargarExtractoAsync();
        await CargarListaAsync();
    }

    // ── Saldos ─────────────────────────────────────────────────
    private async Task CargarSaldosAsync()
    {
        if (!Id.HasValue) return;

        _cargandoSaldos = true;
        try
        {
            var saldos = await PlanCuentasClient.SaldosAsync(Id.Value, _anioSaldos);
            _saldosMensuales = saldos.ToList();
        }
        catch
        {
            _saldosMensuales = [];
        }
        finally
        {
            _cargandoSaldos = false;
        }
    }

    private async Task CambiarAnioSaldosAsync(int anio)
    {
        _anioSaldos = anio;
        await CargarSaldosAsync();
    }

    // ── Búsquedas para ItemSelector ───────────────────────────
    private async Task<IEnumerable<CuentaResponse>> BuscarCuentasAsync(string query, CancellationToken ct)
    {
        var resultado = await PlanCuentasClient.PlanCuentasGETAsync(
            codigo:            null,
            descripcion:       null,
            nif:               null,
            conNif:            null,
            conDescripcionSii: null,
            conSaldo:          null,
            soloConApuntes:    null,
            tipoDiarioId:      null,
            centroCosteId:     null,
            desde:             null,
            hasta:             null,
            esDefinitivo:      null,
            punteado:          null,
            pagina:            0,
            porPagina:         50,
            busqueda:          string.IsNullOrWhiteSpace(query) ? null : query,
            ordenarPor:        nameof(CuentaResponse.Codigo),
            ordenDesc:         false,
            cancellationToken: ct);
        return resultado.Items;
    }

    private async Task<IEnumerable<EmpresaResponse>> BuscarEmpresasAsync(string query, CancellationToken ct)
    {
        var resultado = await EmpresasClient.EmpresasGETAsync(new PaginacionFilter
        {
            Pagina    = 0,
            PorPagina = 50,
            Busqueda  = string.IsNullOrWhiteSpace(query) ? null : query,
        }, ct);
        return resultado.Items;
    }

    // ── Helpers ────────────────────────────────────────────────
    private static string NombreMes(int mes) => mes switch
    {
        1  => "Enero",
        2  => "Febrero",
        3  => "Marzo",
        4  => "Abril",
        5  => "Mayo",
        6  => "Junio",
        7  => "Julio",
        8  => "Agosto",
        9  => "Septiembre",
        10 => "Octubre",
        11 => "Noviembre",
        12 => "Diciembre",
        _  => mes.ToString()
    };
}
