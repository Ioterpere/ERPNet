using ERPNet.ApiClient;

namespace ERPNet.Web.Blazor.Client.Services;

/// <summary>
/// Almacena acciones de prefillado keyed por ruta de página.
/// Singleton para que los datos sobrevivan la navegación entre componentes.
/// </summary>
public class PrefilladoService
{
    private readonly Dictionary<string, AccionUi> _acciones = [];

    /// <summary>Notifica cuando hay nuevos datos de prefillado. El parámetro es la ruta destino.</summary>
    public event Action<string>? OnPrefillado;

    public void Guardar(AccionUi accion)
    {
        if (accion.Ruta is null) return;
        _acciones[accion.Ruta] = accion;
        OnPrefillado?.Invoke(accion.Ruta);
    }

    public AccionUi? Consumir(string ruta)
    {
        if (!_acciones.TryGetValue(ruta, out var accion)) return null;
        _acciones.Remove(ruta);
        return accion;
    }
}
