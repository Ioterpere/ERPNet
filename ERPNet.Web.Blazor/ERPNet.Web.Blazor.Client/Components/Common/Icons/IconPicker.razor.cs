using ERPNet.Web.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;

namespace ERPNet.Web.Blazor.Client.Components.Common.Icons;

public partial class IconPicker : ComponentBase
{
    [Inject] private BootstrapIconsService IconsService { get; set; } = default!;

    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    private bool _abierto;
    private bool _cargando;
    private string _busqueda = "";
    private IReadOnlyList<string> _todosIconos = [];
    private ElementReference _refBusqueda;
    private bool _enfocarBusqueda;

    private IReadOnlyList<string> IconosFiltrados
    {
        get
        {
            if (_todosIconos.Count == 0) return [];
            if (string.IsNullOrWhiteSpace(_busqueda))
                return _todosIconos;
            return _todosIconos
                .Where(i => i.Contains(_busqueda, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private async Task AbrirAsync()
    {
        if (_abierto) { Cerrar(); return; }

        _abierto = true;
        _busqueda = "";

        if (_todosIconos.Count == 0)
        {
            _cargando = true;
            StateHasChanged();
            try { _todosIconos = await IconsService.GetIconNamesAsync(); }
            finally { _cargando = false; }
        }

        _enfocarBusqueda = true;
    }

    private void Cerrar()
    {
        _abierto = false;
        _busqueda = "";
    }

    private async Task SeleccionarAsync(string icono)
    {
        await ValueChanged.InvokeAsync("bi-" + icono);
        Cerrar();
    }

    private async Task LimpiarAsync()
    {
        await ValueChanged.InvokeAsync(null);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_enfocarBusqueda)
        {
            _enfocarBusqueda = false;
            try { await _refBusqueda.FocusAsync(); } catch { }
        }
    }
}
