using ERPNet.ApiClient;

namespace ERPNet.Web.Blazor.Client.Services;

/// <summary>
/// Caché de menús para la sesión WASM (Scoped = singleton por sesión).
/// Deduplicación: si NavMenu y MenuSearch inicializan a la vez, ambos awaitan
/// el mismo Task — solo se hace una llamada HTTP. Si SSR pre-pobló el dato,
/// el Task ya está completado y no hay ninguna llamada HTTP.
/// </summary>
public sealed class MenuStateService
{
    private Task<ICollection<MenuResponse>>? _task;

    /// <summary>
    /// Pre-pobla el caché desde el estado SSR persistido. Los componentes que
    /// llamen a <see cref="ObtenerAsync"/> después obtendrán los datos sin HTTP.
    /// </summary>
    public void Seed(ICollection<MenuResponse> menus)
        => _task = Task.FromResult(menus);

    /// <summary>
    /// Devuelve los menús. La primera llamada lanza la petición HTTP;
    /// las siguientes reutilizan el mismo Task ya en vuelo o completado.
    /// </summary>
    public Task<ICollection<MenuResponse>> ObtenerAsync(IMenusClient client)
        => _task ??= client.MenusAllAsync();

    /// <summary>Invalida el caché para forzar una nueva petición (ej: cambio de empresa).</summary>
    public void Invalidar() => _task = null;
}
