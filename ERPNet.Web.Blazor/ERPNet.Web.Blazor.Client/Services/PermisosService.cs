using ERPNet.ApiClient;

namespace ERPNet.Web.Blazor.Client.Services;

/// <summary>
/// Carga los permisos del usuario autenticado una sola vez por sesión y los expone
/// para que cualquier componente pueda mostrar/ocultar elementos de UI.
/// Se registra como Scoped — en WASM equivale a singleton por sesión.
/// </summary>
public class PermisosService(IAuthClient authClient)
{
    private IReadOnlyList<PermisoResponse>? _cache;

    private async Task<IReadOnlyList<PermisoResponse>> GetAsync()
    {
        if (_cache is not null) return _cache;
        if (!OperatingSystem.IsBrowser()) return _cache = [];
        try
        {
            _cache = [.. await authClient.MisPermisosAsync()];
        }
        catch
        {
            _cache = [];
        }
        return _cache;
    }

    public async Task<bool> TieneAcceso(RecursoCodigo recurso)
        => (await GetAsync()).Any(p => p.Codigo == recurso);

    public async Task<bool> PuedeCrear(RecursoCodigo recurso)
        => (await GetAsync()).Any(p => p.Codigo == recurso && p.CanCreate);

    public async Task<bool> PuedeEditar(RecursoCodigo recurso)
        => (await GetAsync()).Any(p => p.Codigo == recurso && p.CanEdit);

    public async Task<bool> PuedeBorrar(RecursoCodigo recurso)
        => (await GetAsync()).Any(p => p.Codigo == recurso && p.CanDelete);
}
