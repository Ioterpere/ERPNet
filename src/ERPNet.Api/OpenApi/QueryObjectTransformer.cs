using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.RegularExpressions;

namespace ERPNet.Api.OpenApi;

/// <summary>
/// Registra en components/schemas los tipos complejos usados como [FromQuery]
/// (ej: PaginacionFilter, EmpleadoFilter) para que NSwag los genere como clases DTO.
/// Los parámetros individuales del endpoint NO se modifican, solo se añaden los schemas.
/// </summary>
public class QueryObjectTransformer(IApiDescriptionGroupCollectionProvider apiDescriptions)
    : IOpenApiDocumentTransformer
{
    private static readonly Regex _constraintRegex = new(@":\w+(?:\(.*?\))?", RegexOptions.Compiled);

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken ct)
    {
        if (document.Paths is null) return Task.CompletedTask;

        var lookup = BuildLookup();

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        foreach (var (docPath, pathItem) in document.Paths)
        foreach (var (opType, operation) in pathItem.Operations ?? [])
        {
            var key = $"{opType.ToString().ToUpperInvariant()}:{NormalizePath(docPath)}";
            if (!lookup.TryGetValue(key, out var action)) continue;

            foreach (var methodParam in action.MethodInfo.GetParameters())
            {
                var tipo = methodParam.ParameterType;
                if (EsPrimitivo(tipo)) continue;

                var propNames = tipo.GetProperties()
                    .Select(p => p.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var matching = operation.Parameters
                    ?.Where(p => p.In == ParameterLocation.Query && p.Name is not null && propNames.Contains(p.Name))
                    .ToList();

                if (matching is not { Count: > 0 }) continue;

                var schemaName = tipo.Name;

                if (!document.Components.Schemas.ContainsKey(schemaName))
                {
                    var propsByName = tipo.GetProperties()
                        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

                    document.Components.Schemas[schemaName] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = matching
                            .Where(p => p.Name is not null && p.Schema is not null)
                            .ToDictionary(p => p.Name!, p => NullableSchema(p.Schema!, propsByName.GetValueOrDefault(p.Name!)))
                    };
                }
            }
        }

        return Task.CompletedTask;
    }

    private Dictionary<string, ControllerActionDescriptor> BuildLookup()
    {
        var dict = new Dictionary<string, ControllerActionDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in apiDescriptions.ApiDescriptionGroups.Items)
        foreach (var desc in group.Items)
        {
            if (desc.ActionDescriptor is not ControllerActionDescriptor action) continue;
            var path = NormalizePath("/" + desc.RelativePath?.TrimStart('/'));
            var method = (desc.HttpMethod ?? "GET").ToUpperInvariant();
            dict.TryAdd($"{method}:{path}", action);
        }
        return dict;
    }

    private static string NormalizePath(string path)
        => _constraintRegex.Replace(path, string.Empty).ToLowerInvariant();

    private static IOpenApiSchema NullableSchema(IOpenApiSchema schema, System.Reflection.PropertyInfo? prop)
    {
        if (prop is null) return schema;
        if (Nullable.GetUnderlyingType(prop.PropertyType) is null) return schema;
        // Es Nullable<T>: añadir null al tipo para que NSwag genere T?
        if (schema is OpenApiSchema s && (s.Type & JsonSchemaType.Null) == 0)
            return new OpenApiSchema { Type = s.Type | JsonSchemaType.Null, Format = s.Format };
        return schema;
    }

    private static bool EsPrimitivo(Type t)
    {
        var core = Nullable.GetUnderlyingType(t) ?? t;
        return core.IsPrimitive || core == typeof(string) || core == typeof(decimal) || core.IsEnum;
    }
}
