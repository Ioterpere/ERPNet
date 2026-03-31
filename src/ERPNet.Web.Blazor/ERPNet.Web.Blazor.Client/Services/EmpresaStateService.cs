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

    /// <summary>Lista de empresas accesibles por el usuario. Vacía hasta que NavMenu la cargue.</summary>
    public IReadOnlyList<EmpresaItem> Empresas { get; private set; } = [];

    /// <summary>Se dispara cuando el usuario cambia de empresa.</summary>
    public event Action? OnCambio;

    /// <summary>Se dispara cuando la lista de empresas está disponible.</summary>
    public event Action? OnEmpresasLoaded;

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
    /// Almacena la lista de empresas accesibles y notifica a los suscriptores.
    /// Llamado por NavMenu tras cargarlas de la API.
    /// </summary>
    public void SetEmpresas(List<EmpresaItem> empresas)
    {
        Empresas = empresas;
        OnEmpresasLoaded?.Invoke();
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

public sealed record EmpresaItem(int Id, string Nombre, string? Cif, bool Activo);
