namespace ERPNet.Web.Blazor.Client.Services;

/// <summary>
/// Fuente de verdad de la empresa activa en el cliente (Scoped = singleton por sesión WASM).
/// Permite cambiar de empresa sin recargar el runtime WASM (forceLoad).
/// MainLayout y NavMenu se suscriben a <see cref="OnCambio"/> para actualizar la UI.
/// </summary>
public sealed class EmpresaStateService
{
    public int? EmpresaId { get; private set; }
    public string? EmpresaNombre { get; private set; }

    /// <summary>Se dispara cuando el usuario cambia de empresa.</summary>
    public event Action? OnCambio;

    /// <summary>
    /// Inicializa desde los claims de AuthenticationState en el primer render.
    /// No dispara <see cref="OnCambio"/> porque es la carga inicial.
    /// </summary>
    public void Inicializar(int? empresaId, string? empresaNombre)
    {
        if (EmpresaId is not null) return; // ya inicializado
        EmpresaId = empresaId;
        EmpresaNombre = empresaNombre;
    }

    /// <summary>
    /// Cambia la empresa activa y notifica a los suscriptores.
    /// </summary>
    public void Cambiar(int empresaId, string empresaNombre)
    {
        EmpresaId = empresaId;
        EmpresaNombre = empresaNombre;
        OnCambio?.Invoke();
    }
}
