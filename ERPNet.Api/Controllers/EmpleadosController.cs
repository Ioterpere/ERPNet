using ERPNet.Api.Attributes;
using ERPNet.Application.Reports.DTOs;
using ERPNet.Application.Reports.Interfaces;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Empleados)]
public class EmpleadosController(IReporteEmpleadoService reporteService) : BaseController
{
    [HttpGet("reporte")]
    public async Task<IActionResult> Reporte([FromQuery] EmpleadoReporteFilter filter, CancellationToken ct = default)
    {
        var result = await reporteService.GenerarAsync(filter, ct);

        if (!result.IsSuccess)
            return FromResult(result);

        var archivo = result.Value!;
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }
}
