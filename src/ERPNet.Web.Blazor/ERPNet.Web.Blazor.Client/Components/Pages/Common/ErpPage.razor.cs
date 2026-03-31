using ERPNet.Web.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Common;

public partial class ErpPage
{
    [Inject] private EmpresaStateService EmpresaState { get; set; } = default!;

    // ── Obligatorios ──────────────────────────────────────────────
    [Parameter, EditorRequired] public string Titulo { get; set; } = string.Empty;
    [Parameter, EditorRequired] public bool EsNuevo { get; set; }
    [Parameter, EditorRequired] public bool TieneDetalle { get; set; }
    [Parameter, EditorRequired] public string Busqueda { get; set; } = string.Empty;
    [Parameter, EditorRequired] public EventCallback<ChangeEventArgs> OnBusquedaInput { get; set; }
    [Parameter, EditorRequired] public EventCallback OnNuevo { get; set; }
    [Parameter, EditorRequired] public EventCallback OnVolverALista { get; set; }
    [Parameter, EditorRequired] public RenderFragment ListaBody { get; set; } = default!;

    // ── Opcionales con default ────────────────────────────────────
    [Parameter] public bool ConTabs { get; set; }
    [Parameter] public int ColLista { get; set; } = 4;
    [Parameter] public string SearchPlaceholder { get; set; } = "Buscar...";
    [Parameter] public string? TituloNuevo { get; set; }
    [Parameter] public string PlaceholderIcono { get; set; } = "bi-inbox";
    [Parameter] public string PlaceholderTexto { get; set; } = "Selecciona un elemento de la lista";
    [Parameter] public int? TotalItems { get; set; }

    // ── Modal eliminar ────────────────────────────────────────────
    [Parameter] public bool MostrarModalEliminar { get; set; }
    [Parameter] public EventCallback<bool> MostrarModalEliminarChanged { get; set; }
    [Parameter] public string? TextoEliminar { get; set; }
    [Parameter] public string? ErrorEliminar { get; set; }
    [Parameter] public bool Eliminando { get; set; }
    [Parameter] public EventCallback OnEliminar { get; set; }

    // ── Slots opcionales ──────────────────────────────────────────
    [Parameter] public RenderFragment? AccionesHeader { get; set; }
    [Parameter] public RenderFragment? SearchExtras { get; set; }
    [Parameter] public RenderFragment? ListaFooterExtra { get; set; }
    [Parameter] public RenderFragment? FormNuevo { get; set; }
    [Parameter] public RenderFragment? FormEdicion { get; set; }
    [Parameter] public RenderFragment? DetailToolbar { get; set; }
    [Parameter] public RenderFragment? ModalesExtra { get; set; }

    // ── NombreEmpresa ─────────────────────────────────────────────
    private string _empresaNombre => EmpresaState.EmpresaNombre ?? "ERPNet";

    // ── Referencias internas ──────────────────────────────────────
    private ElementReference _refSearch;
    private ElementReference _refBtnEliminar;

    private bool MostrarLista => !EsNuevo && !TieneDetalle;

    public Task FocusSearchAsync()      => _refSearch.FocusAsync().AsTask();
    public Task FocusBtnEliminarAsync() => _refBtnEliminar.FocusAsync().AsTask();
}
