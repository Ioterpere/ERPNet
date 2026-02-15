using ERPNet.Api.Attributes;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using ERPNet.Api.Controllers.Common;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class MenusController(IMenuService menuService) : BaseController
{
    [HttpGet]
    [SinPermiso]
    public async Task<IActionResult> GetMenus([FromQuery] Plataforma plataforma)
        => FromResult(await menuService.GetMenusVisiblesAsync(plataforma, UsuarioActual.RolIds));

    [HttpGet("{id}", Name = nameof(GetMenuById))]
    public async Task<IActionResult> GetMenuById(int id)
        => FromResult(await menuService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request)
        => CreatedFromResult(
            await menuService.CreateAsync(request),
            nameof(GetMenuById),
            r => new { id = r.Id });

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetRoles(int id)
        => FromResult(await menuService.GetRolesAsync(id));

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> AsignarRoles(int id, [FromBody] AsignarRolesRequest request)
        => FromResult(await menuService.AsignarRolesAsync(id, request));
}
