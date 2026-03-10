using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class MenuService(
    IMenuRepository menuRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IMenuService
{
    public async Task<Result<List<MenuResponse>>> GetMenusVisiblesAsync(Plataforma plataforma, List<int> rolIds)
    {
        var menus = await menuRepository.GetMenusVisiblesAsync(plataforma, rolIds);
        var response = menus.Select(m => m.ToResponse()).ToList();
        return Result<List<MenuResponse>>.Success(response);
    }

    public async Task<Result<List<MenuResponse>>> GetAllAdminAsync(Plataforma plataforma)
    {
        var menus = await menuRepository.GetAllAdminAsync(plataforma);
        return Result<List<MenuResponse>>.Success(menus.Select(m => m.ToResponse()).ToList());
    }

    public async Task<Result<MenuResponse>> GetByIdAsync(int id)
    {
        var menu = await menuRepository.GetByIdAsync(id);

        if (menu is null)
            return Result<MenuResponse>.Failure("Menú no encontrado.", ErrorType.NotFound);

        return Result<MenuResponse>.Success(menu.ToResponse());
    }

    public async Task<Result<MenuResponse>> CreateAsync(CreateMenuRequest request)
    {
        var menu = request.ToEntity();

        await menuRepository.CreateAsync(menu);
        await unitOfWork.SaveChangesAsync();

        return Result<MenuResponse>.Success(menu.ToResponse());
    }

    public async Task<Result<MenuResponse>> UpdateAsync(int id, UpdateMenuRequest request)
    {
        var menu = await menuRepository.GetByIdAsync(id);

        if (menu is null)
            return Result<MenuResponse>.Failure("Menú no encontrado.", ErrorType.NotFound);

        request.ApplyTo(menu);
        await unitOfWork.SaveChangesAsync();

        return Result<MenuResponse>.Success(menu.ToResponse());
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var menu = await menuRepository.GetByIdAsync(id);

        if (menu is null)
            return Result.Failure("Menú no encontrado.", ErrorType.NotFound);

        if (await menuRepository.TieneSubMenusAsync(id))
            return Result.Failure("No se puede eliminar un menú que tiene submenús.", ErrorType.Conflict);

        var rolIds = await menuRepository.GetRolIdsAsync(id);
        menuRepository.Delete(menu);
        await unitOfWork.SaveChangesAsync();

        await InvalidarCacheUsuariosAsync(rolIds);
        return Result.Success();
    }

    public async Task<Result> MoverAsync(int id, MoverMenuRequest request)
    {
        var menu = await menuRepository.GetByIdAsync(id);

        if (menu is null)
            return Result.Failure("Menú no encontrado.", ErrorType.NotFound);

        // Si cambia de padre, reordenar los hermanos del padre original
        if (menu.MenuPadreId != request.MenuPadreId)
        {
            var hermanosAnteriores = await menuRepository.GetHermanosAsync(menu.MenuPadreId, menu.Plataforma);
            hermanosAnteriores.RemoveAll(m => m.Id == id);
            for (var i = 0; i < hermanosAnteriores.Count; i++)
                hermanosAnteriores[i].Orden = i + 1;
        }

        // Obtener los hermanos del destino (tracked), excluyendo el nodo movido
        var hermanos = await menuRepository.GetHermanosAsync(request.MenuPadreId, menu.Plataforma);
        hermanos.RemoveAll(m => m.Id == id);

        // Actualizar el padre y reinsertar en la posición indicada
        menu.MenuPadreId = request.MenuPadreId;
        var pos = Math.Clamp(request.Orden - 1, 0, hermanos.Count);
        hermanos.Insert(pos, menu);

        // Reasignar Orden consecutivo a todos los hermanos
        for (var i = 0; i < hermanos.Count; i++)
            hermanos[i].Orden = i + 1;

        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<int>>> GetRolesAsync(int menuId)
    {
        var menu = await menuRepository.GetByIdAsync(menuId);

        if (menu is null)
            return Result<List<int>>.Failure("Menú no encontrado.", ErrorType.NotFound);

        var rolIds = await menuRepository.GetRolIdsAsync(menuId);
        return Result<List<int>>.Success(rolIds);
    }

    public async Task<Result> AsignarRolesAsync(int menuId, AsignarRolesRequest request)
    {
        var menu = await menuRepository.GetByIdAsync(menuId);

        if (menu is null)
            return Result.Failure("Menú no encontrado.", ErrorType.NotFound);

        var rolesAnteriores = await menuRepository.GetRolIdsAsync(menuId);

        await menuRepository.SincronizarRolesAsync(menuId, request.RolIds);
        await unitOfWork.SaveChangesAsync();

        var rolesAfectados = rolesAnteriores.Union(request.RolIds).ToList();
        await InvalidarCacheUsuariosAsync(rolesAfectados);

        return Result.Success();
    }

    private async Task InvalidarCacheUsuariosAsync(List<int> rolIds)
    {
        var usuarioIds = await menuRepository.GetUsuarioIdsPorRolesAsync(rolIds);
        foreach (var uid in usuarioIds)
            cache.Remove($"usuario:{uid}");
    }
}
