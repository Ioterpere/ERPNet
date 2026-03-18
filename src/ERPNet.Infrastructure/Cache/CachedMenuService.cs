using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Cache;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;

namespace ERPNet.Infrastructure.Cache;

public sealed class CachedMenuService(
    IMenuService inner,
    ICacheService cache,
    ICurrentUserProvider currentUser) : IMenuService
{
    private const string MenuPrefix = "menu:";

    public async Task<Result<List<MenuResponse>>> GetMenusVisiblesAsync(List<int> rolIds)
    {
        var ctx = currentUser.Current;
        if (ctx is null)
            return await inner.GetMenusVisiblesAsync(rolIds);

        var key = $"{MenuPrefix}{ctx.Id}:{ctx.EmpresaId ?? 0}";
        var cached = cache.Get<List<MenuResponse>>(key);
        if (cached is not null)
            return Result<List<MenuResponse>>.Success(cached);

        var result = await inner.GetMenusVisiblesAsync(rolIds);
        if (result.IsSuccess)
            cache.Set(key, result.Value!);

        return result;
    }

    public async Task<Result<MenuResponse>> CreateAsync(CreateMenuRequest request)
    {
        var result = await inner.CreateAsync(request);
        if (result.IsSuccess) cache.RemoveByPrefix(MenuPrefix);
        return result;
    }

    public async Task<Result<MenuResponse>> UpdateAsync(int id, UpdateMenuRequest request)
    {
        var result = await inner.UpdateAsync(id, request);
        if (result.IsSuccess) cache.RemoveByPrefix(MenuPrefix);
        return result;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var result = await inner.DeleteAsync(id);
        if (result.IsSuccess) cache.RemoveByPrefix(MenuPrefix);
        return result;
    }

    public async Task<Result> MoverAsync(int id, MoverMenuRequest request)
    {
        var result = await inner.MoverAsync(id, request);
        if (result.IsSuccess) cache.RemoveByPrefix(MenuPrefix);
        return result;
    }

    public async Task<Result> AsignarRolesAsync(int menuId, AsignarRolesRequest request)
    {
        var result = await inner.AsignarRolesAsync(menuId, request);
        if (result.IsSuccess) cache.RemoveByPrefix(MenuPrefix);
        return result;
    }

    // Delegaciones sin lógica de caché
    public Task<Result<List<MenuResponse>>> GetAllAdminAsync(Plataforma? plataforma = null)
        => inner.GetAllAdminAsync(plataforma);

    public Task<Result<MenuResponse>> GetByIdAsync(int id)
        => inner.GetByIdAsync(id);

    public Task<Result<List<int>>> GetRolesAsync(int menuId)
        => inner.GetRolesAsync(menuId);

    public Task<Result<List<MenuResponse>>> BuscarEnMenuAsync(string busqueda)
        => inner.BuscarEnMenuAsync(busqueda);
}
