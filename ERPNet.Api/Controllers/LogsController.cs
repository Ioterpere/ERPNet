using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Contracts;
using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class LogsController(ILogRepository logRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] LogFilter request)
    {
        var (logs, total) = await logRepository.GetFilteredAsync(request);
        var response = logs.Select(l => l.ToResponse()).ToList();
        return FromResult(Result<ListaPaginada<LogResponse>>.Success(
            ListaPaginada<LogResponse>.Crear(response, total, request.Pagina, request.PorPagina)));
    }
}
