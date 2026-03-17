using System.Linq.Expressions;
using System.Text.Json;
using ERPNet.Application.Ai.DTOs;
using ERPNet.Domain.Enums;

namespace ERPNet.Application.Ai;

public abstract class FormularioToolConfig
{
    public abstract string Nombre { get; }
    public abstract string Descripcion { get; }
    public abstract IReadOnlyDictionary<string, string> Descripciones { get; }
    public abstract RecursoCodigo Recurso { get; }
    public abstract Delegate ConstruirInvocable(IAccionesUiCollector collector);
}

public abstract class FormularioToolConfig<TRequest> : FormularioToolConfig
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly Dictionary<string, string> _descripciones = new(StringComparer.OrdinalIgnoreCase);

    public override IReadOnlyDictionary<string, string> Descripciones => _descripciones;

    protected FormularioToolConfig()
    {
        _descripciones["ruta"] = "Ruta de navegación de la página, obtenida previamente con BuscarRutaEnMenu.";
        Configurar();
    }

    protected abstract void Configurar();

    protected void Describir<TProp>(Expression<Func<TRequest, TProp>> selector, string descripcion)
    {
        var body = selector.Body is UnaryExpression u ? u.Operand : selector.Body;
        _descripciones[((MemberExpression)body).Member.Name] = descripcion;
    }

    public override Delegate ConstruirInvocable(IAccionesUiCollector collector) =>
        (TRequest request, string ruta) =>
        {
            collector.Guardar(new AccionUi
            {
                Tipo     = TipoAccionUi.RellenarFormulario,
                TipoDato = typeof(TRequest).Name,
                Ruta     = ruta,
                Datos    = JsonSerializer.SerializeToElement(request, JsonOpts)
            });
            return "Formulario mostrado en pantalla.";
        };
}
