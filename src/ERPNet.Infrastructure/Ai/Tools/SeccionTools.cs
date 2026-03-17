using System.ComponentModel;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Infrastructure.Ai.Tools.Common;

namespace ERPNet.Infrastructure.Ai.Tools;

public class SeccionTools(ISeccionService service) : McpToolBase
{
    public override RecursoCodigo Recurso => RecursoCodigo.Empleados;

    [Description("Lista todas las secciones disponibles con su ID y nombre. Úsalo para resolver el seccionId antes de llamar a RellenarFormularioEmpleado.")]
    public async Task<string> BuscarSecciones() =>
        FromResult(await service.GetAllAsync());
}
