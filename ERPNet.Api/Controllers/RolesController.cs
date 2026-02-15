using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class RolesController(IRolService rolService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await rolService.GetAllAsync(filtro));

    [HttpGet("{id}", Name = nameof(GetRolById))]
    public async Task<IActionResult> GetRolById(int id)
        => FromResult(await rolService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRolRequest request)
        => CreatedFromResult(
            await rolService.CreateAsync(request),
            nameof(GetRolById),
            r => new { id = r.Id });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRolRequest request)
        => FromResult(await rolService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await rolService.DeleteAsync(id));
}
