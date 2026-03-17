using ERPNet.Application.Ai;
using ERPNet.Infrastructure.Ai.Tools.Common;
using ERPNet.Application.Ai.DTOs;
using ERPNet.Application.Ai.ToolConfigs;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using Microsoft.Extensions.AI;

namespace ERPNet.Infrastructure.Ai.Tools;

public class EmpleadoTools(IEmpleadoService service, EmpleadoConsulta config, IAccionesUiCollector accionesUi)
    : QueryableToolBase(config, accionesUi)
{
    public override RecursoCodigo Recurso => RecursoCodigo.Empleados;

    public override IEnumerable<AITool> GetAiFunctions()
    {
        yield return BuildBuscarFunction(BuscarAsync);
        yield return BuildMostrarOpcionesFunction();
        yield return BuildSeleccionarFunction();
    }

    private Task<string> BuscarAsync(EmpleadoFilter filtro) =>
        EjecutarBusquedaAsync(service.GetAllAsync(filtro));
}
