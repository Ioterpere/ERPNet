using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class MenuMappings
{
    public static MenuResponse ToResponse(this Menu menu) => new()
    {
        Id = menu.Id,
        Nombre = menu.Nombre,
        Path = menu.Path,
        IconClass = menu.IconClass,
        CustomClass = menu.CustomClass,
        Orden = menu.Orden,
        SubMenus = menu.SubMenus.Select(s => s.ToResponse()).ToList()
    };

    public static Menu ToEntity(this CreateMenuRequest request) => new()
    {
        Nombre = request.Nombre,
        Path = request.Path,
        IconClass = request.IconClass,
        CustomClass = request.CustomClass,
        Orden = request.Orden,
        Plataforma = request.Plataforma,
        MenuPadreId = request.MenuPadreId,
        RecursoId = request.RecursoId
    };
}
