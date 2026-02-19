using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class MenuMappings
{

    public static MenuResponse ToResponse(this Menu menu) => new()
    {
        Id = menu.Id,
        Nombre = menu.Nombre,
        Path = menu.Path,
        IconClass = menu.Icon,
        CustomClass = menu.Tag,
        Orden = menu.Orden,
        SubMenus = menu.SubMenus.Select(s => s.ToResponse()).ToList()
    };

    public static Menu ToEntity(this CreateMenuRequest request, Plataforma plataforma) => new()
    {
        Nombre = request.Nombre,
        Path = request.Path,
        Icon = request.IconClass,
        Tag = request.CustomClass,
        Orden = request.Orden,
        Plataforma = plataforma,
        MenuPadreId = request.MenuPadreId,
        MenusRoles = request.RolIds.Select(id => new MenuRol { RolId = id }).ToList()
    };
}
