using System.Collections.Frozen;
using System.Reflection;
using ERPNet.ApiClient;

namespace ERPNet.Web.Blazor.Client.Components.Common;

public class FilterState<T> where T : class, new()
{
    private static readonly FrozenSet<string> _excluir =
        typeof(PaginacionFilter)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly T _default = new();
    private static readonly PropertyInfo[] _props =
        typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    public T Aplicado  { get; private set; } = new();
    public T Editando  { get; private set; } = new();
    public bool ModalVisible { get; set; }

    public int Count => _props
        .Where(p => !_excluir.Contains(p.Name))
        .Count(p => !Equals(p.GetValue(Aplicado), p.GetValue(_default)));

    public bool HayFiltros => Count > 0;

    public void Abrir()    { Editando = Copiar(Aplicado); ModalVisible = true; }
    public void Aplicar()  { Aplicado = Editando; ModalVisible = false; }
    public void Cancelar() { ModalVisible = false; }
    public void Limpiar()  { Aplicado = new(); Editando = new(); ModalVisible = false; }

    /// <summary>Devuelve una copia de Aplicado con campos adicionales sobreescritos (paginación, orden…).</summary>
    public T AplicadoCon(Action<T> configurar)
    {
        var copia = Copiar(Aplicado);
        configurar(copia);
        return copia;
    }

    private static T Copiar(T src)
    {
        var dst = new T();
        foreach (var p in _props)
            if (p.CanWrite) p.SetValue(dst, p.GetValue(src));
        return dst;
    }
}
