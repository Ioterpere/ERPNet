using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.FileStorage;
using ERPNet.Application.Reports.DTOs;
using ERPNet.Application.Reports.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Empleados)]
public class EmpleadosController(
    IFileStorageService fileStorage,
    IEmpleadoRepository repo,
    IUnitOfWork unitOfWork,
    IReporteEmpleadoService reporteService,
    IEmpleadoService empleadoService)
    : ArchivoBaseController<Empleado, CampoArchivoEmpleado>(fileStorage, repo, unitOfWork)
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await empleadoService.GetAllAsync(filtro));

    [HttpGet("{id}", Name = nameof(GetEmpleadoById))]
    public async Task<IActionResult> GetEmpleadoById(int id)
        => FromResult(await empleadoService.GetByIdAsync(id));

    [SinPermiso]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
        => FromResult(await empleadoService.GetMeAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmpleadoRequest request)
        => CreatedFromResult(
            await empleadoService.CreateAsync(request),
            nameof(GetEmpleadoById),
            r => new { id = r.Id });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmpleadoRequest request)
        => FromResult(await empleadoService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await empleadoService.DeleteAsync(id));

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
