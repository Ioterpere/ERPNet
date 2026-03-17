using System.Linq.Expressions;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Ai;

public abstract class ConsultaToolConfig
{
    public abstract string Entidad { get; }
    public abstract string Nombre { get; }
    public abstract string Descripcion { get; }
    public abstract IReadOnlyDictionary<string, string> Descripciones { get; }

    /// <summary>Nombre del tool de selección (ej: "SeleccionarEmpleado"). Derivado de Entidad por defecto.</summary>
    public virtual string NombreSeleccionar =>
        "Seleccionar" + char.ToUpper(Entidad[0]) + Entidad[1..].TrimEnd('s');

    /// <summary>Nombre del tool de mostrar opciones (ej: "MostrarOpcionesEmpleados"). Derivado de Entidad por defecto.</summary>
    public virtual string NombreMostrarOpciones =>
        "MostrarOpciones" + char.ToUpper(Entidad[0]) + Entidad[1..];

    /// <summary>Descripción del tool de selección para el modelo.</summary>
    public virtual string DescripcionSeleccionar =>
        $"Navega al detalle de un registro de {Entidad}. Llama primero a BuscarRutaEnMenu para obtener la ruta. Úsalo cuando conozcas el ID exacto o tras disambiguar.";
}

public abstract class ConsultaToolConfig<TFilter> : ConsultaToolConfig
    where TFilter : PaginacionFilter
{
    private readonly Dictionary<string, string> _descripciones = new(StringComparer.OrdinalIgnoreCase);

    public override IReadOnlyDictionary<string, string> Descripciones => _descripciones;

    protected ConsultaToolConfig() => Configurar();

    protected abstract void Configurar();

    protected void Describir<TProp>(Expression<Func<TFilter, TProp>> selector, string descripcion)
    {
        var body = selector.Body is UnaryExpression u ? u.Operand : selector.Body;
        _descripciones[((MemberExpression)body).Member.Name] = descripcion;
    }
}
