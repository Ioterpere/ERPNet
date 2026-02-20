using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

public class SeccionesController(ISeccionService service) : BaseController
{
    [HttpGet]
    [ProducesResponseType<List<SeccionResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => FromResult(await service.GetAllAsync());
}
