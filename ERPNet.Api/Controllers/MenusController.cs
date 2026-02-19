using ERPNet.Api.Attributes;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using ERPNet.Api.Controllers.Common;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class MenusController(IMenuService menuService) : BaseController
{
    [HttpGet]
    [SinPermiso]
    [ProducesResponseType<List<MenuResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMenus([FromQuery] Plataforma plataforma)
        => FromResult(await menuService.GetMenusVisiblesAsync(plataforma, UsuarioActual.RolIds));

    [HttpGet("{id}")]
    [ProducesResponseType<MenuResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await menuService.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<MenuResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request)
        => CreatedFromResult(await menuService.CreateAsync(request));

    [HttpGet("{id}/roles")]
    [ProducesResponseType<List<int>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(int id)
        => FromResult(await menuService.GetRolesAsync(id));

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> AsignarRoles(int id, [FromBody] AsignarRolesRequest request)
        => FromResult(await menuService.AsignarRolesAsync(id, request));
}
