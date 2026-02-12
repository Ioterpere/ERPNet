using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.FileStorage;
using ERPNet.Application.Reports.DTOs;
using ERPNet.Application.Reports.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Empleados)]
public class EmpleadosController(
    IFileStorageService fileStorage,
    IEmpleadoRepository repo,
    IUnitOfWork unitOfWork,
    IReporteEmpleadoService reporteService)
    : ArchivoBaseController<Empleado, CampoArchivoEmpleado>(fileStorage, repo, unitOfWork)
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
