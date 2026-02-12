using ERPNet.Api.Attributes;
using ERPNet.Application.DTOs;
using ERPNet.Application.DTOs.Mappings;
using ERPNet.Domain.Repositories;
using ERPNet.Application;
using ERPNet.Application.Enums;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using ERPNet.Api.Controllers.Common;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class MenusController(
    IMenuRepository menuRepository,
    IUnitOfWork unitOfWork) : BaseController
{
    [HttpGet]
    [SinPermiso]
    public async Task<IActionResult> GetMenus([FromQuery] Plataforma plataforma)
    {
        var codigos = UsuarioActual.Permisos.Select(p => p.Codigo).ToList();
        var menus = await menuRepository.GetMenusVisiblesAsync(plataforma, codigos);
        var response = menus.Select(m => m.ToResponse()).ToList();
        return FromResult(Result<List<MenuResponse>>.Success(response));
    }

    [HttpGet("{id}", Name = nameof(GetMenuById))]
    public async Task<IActionResult> GetMenuById(int id)
    {
        var menu = await menuRepository.GetByIdAsync(id);

        if (menu is null)
            return FromResult(Result.Failure("Menu no encontrado.", ErrorType.NotFound));

        return FromResult(Result<MenuResponse>.Success(menu.ToResponse()));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request)
    {
        var menu = request.ToEntity();

        await menuRepository.CreateAsync(menu);
        await unitOfWork.SaveChangesAsync();

        return CreatedFromResult(
            Result<MenuResponse>.Success(menu.ToResponse()),
            nameof(GetMenuById),
            new { id = menu.Id });
    }
}
