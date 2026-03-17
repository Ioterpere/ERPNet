using System.Text.Json.Nodes;
using ERPNet.Application.Ai;
using ERPNet.Application.Ai.DTOs;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;
using Microsoft.Extensions.AI;

namespace ERPNet.Infrastructure.Ai.Tools.Common;

public abstract class QueryableToolBase(ConsultaToolConfig config, IAccionesUiCollector accionesUi)
    : McpToolBase
{
    protected AITool BuildBuscarFunction(Delegate buscar) =>
        AIFunctionFactory.Create(buscar, BuildBuscarOptions());

    protected AIFunctionFactoryOptions BuildBuscarOptions() => new()
    {
        Name = config.Nombre,
        Description = config.Descripcion,
        JsonSchemaCreateOptions = new AIJsonSchemaCreateOptions
        {
            TransformSchemaNode = (ctx, node) =>
            {
                if (ctx.PropertyInfo?.Name is { } nombre &&
                    config.Descripciones.TryGetValue(nombre, out var desc) &&
                    node is JsonObject obj)
                    obj["description"] = desc;
                return node;
            }
        }
    };

    protected AIFunction BuildMostrarOpcionesFunction()
    {
        var entidad = config.Entidad;
        return AIFunctionFactory.Create(
            (List<ItemSeleccionable> opciones) =>
            {
                accionesUi.Guardar(new AccionUi
                {
                    Tipo     = TipoAccionUi.ElegirOpcion,
                    TipoDato = entidad,
                    Opciones = opciones
                });
                return "Opciones mostradas al usuario.";
            },
            new AIFunctionFactoryOptions
            {
                Name = config.NombreMostrarOpciones,
                Description = $"Muestra una lista de opciones de {entidad} al usuario para que elija. " +
                              "Tú decides el formato de Etiqueta según el contexto."
            });
    }

    protected AIFunction BuildSeleccionarFunction() =>
        AIFunctionFactory.Create(
            (int id, string ruta) =>
            {
                accionesUi.Guardar(new AccionUi
                {
                    Tipo     = TipoAccionUi.AbrirRegistro,
                    TipoDato = config.Entidad,
                    Ruta     = ruta,
                    Datos    = id
                });
                return "Registro seleccionado.";
            },
            new AIFunctionFactoryOptions
            {
                Name = config.NombreSeleccionar,
                Description = config.DescripcionSeleccionar
            });

    protected async Task<string> EjecutarBusquedaAsync<T>(Task<Result<ListaPaginada<T>>> busqueda)
    {
        var r = await busqueda;
        if (!r.IsSuccess) return Error(r.Error ?? "Error desconocido");
        var items = r.Value!.Items;
        if (items.Count > 1)
            return Ok(new
            {
                encontrados = items.Count,
                items,
                accion = $"Múltiples resultados. Debes llamar a {config.NombreMostrarOpciones} con estos items. Tú decides la Etiqueta de cada uno."
            });
        return Ok(new { encontrados = items.Count, items });
    }
}
