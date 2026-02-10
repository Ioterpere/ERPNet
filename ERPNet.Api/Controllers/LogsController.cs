using ERPNet.Api.Attributes;
using ERPNet.Application;
using ERPNet.Application.DTOs;
using ERPNet.Application.DTOs.Mappings;
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
        var logs = await logRepository.GetFilteredAsync(request);
        var response = logs.Select(l => l.ToResponse()).ToList();
        return FromResult(Result<List<LogResponse>>.Success(response));
    }
}
