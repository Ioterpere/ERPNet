using System.ComponentModel;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Infrastructure.Ai.Tools.Common;
using Microsoft.Extensions.AI;

namespace ERPNet.Infrastructure.Ai.Tools;

public class MenuTools(IMenuService service) : McpToolBase
{
    public override RecursoCodigo Recurso => RecursoCodigo.Aplicacion;

    public override IEnumerable<AITool> GetAiFunctions()
    {
        yield return AIFunctionFactory.Create(BuscarRutaEnMenuAsync,
            new AIFunctionFactoryOptions { Name = "BuscarRutaEnMenu" });
    }

    [Description("Obtiene la ruta de navegación de una página del menú del usuario. " +
                 "Úsalo para obtener la ruta antes de llamar a cualquier SeleccionarX.")]
    private async Task<string> BuscarRutaEnMenuAsync(
        [Description("Término de búsqueda: nombre del módulo o entidad (ej: 'empleados', 'maquinaria').")] string busqueda)
    {
        var r = await service.BuscarEnMenuAsync(busqueda);
        if (!r.IsSuccess) return Error("No se pudo obtener el menú.");

        return r.Value!.Count == 0
            ? Error($"No se encontró ninguna página para '{busqueda}'.")
            : Ok(r.Value!.Select(m => new { m.Nombre, Ruta = m.Path }));
    }
}
