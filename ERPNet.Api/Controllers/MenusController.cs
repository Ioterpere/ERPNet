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

    [HttpGet("admin")]
    [ProducesResponseType<List<MenuResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAdmin([FromQuery] Plataforma plataforma = Plataforma.WebBlazor)
        => FromResult(await menuService.GetAllAdminAsync(plataforma));

    [HttpGet("{id}")]
    [ProducesResponseType<MenuResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await menuService.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<MenuResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request)
        => CreatedFromResult(await menuService.CreateAsync(request));

    [HttpPut("{id}")]
    [ProducesResponseType<MenuResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuRequest request)
        => FromResult(await menuService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await menuService.DeleteAsync(id));

    [HttpPut("{id}/mover")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Mover(int id, [FromBody] MoverMenuRequest request)
        => FromResult(await menuService.MoverAsync(id, request));

    [HttpGet("{id}/roles")]
    [ProducesResponseType<List<int>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(int id)
        => FromResult(await menuService.GetRolesAsync(id));

    [HttpPut("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AsignarRoles(int id, [FromBody] AsignarRolesRequest request)
        => FromResult(await menuService.AsignarRolesAsync(id, request));
}
