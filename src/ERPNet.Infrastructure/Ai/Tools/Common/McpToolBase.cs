using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using ERPNet.Application.Common;
using ERPNet.Domain.Enums;
using Microsoft.Extensions.AI;

namespace ERPNet.Infrastructure.Ai.Tools.Common;

public abstract class McpToolBase
{
    public abstract RecursoCodigo Recurso { get; }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected static JsonElement SerializeToElement<T>(T value) =>
        JsonSerializer.SerializeToElement(value, JsonOpts);

    protected static string Ok<T>(T data) =>
        JsonSerializer.Serialize(new { ok = true, data }, JsonOpts);

    protected static string Error(string mensaje) =>
        JsonSerializer.Serialize(new { ok = false, error = mensaje }, JsonOpts);

    protected static string FromResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Error(result.Error ?? "Error desconocido");

    protected static string FromResult(Result result) =>
        result.IsSuccess ? Ok((object?)null) : Error(result.Error ?? "Error desconocido");

    /// <summary>
    /// Devuelve las AIFunctions que este tool expone al modelo.
    /// Por defecto: todos los métodos públicos con [Description].
    /// Las subclases pueden sobreescribir para construir funciones dinámicamente.
    /// </summary>
    public virtual IEnumerable<AITool> GetAiFunctions() =>
        GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() is not null)
            .Select(m => AIFunctionFactory.Create(m, this));
}
