using System.Linq.Expressions;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class MenuRepository(ERPNetDbContext context, ICurrentUserProvider currentUser) : Repository<Menu>(context, currentUser), IMenuRepository
{
    protected override Expression<Func<Menu, bool>>? GetBusquedaPredicate(string busqueda)
        => m => m.Nombre.Contains(busqueda);

    public async Task<List<Menu>> GetAllAdminAsync(Plataforma plataforma)
    {
        return await Context.Menus
            .Include(m => m.SubMenus.OrderBy(s => s.Orden))
                .ThenInclude(s => s.SubMenus.OrderBy(ss => ss.Orden))
            .Where(m => m.Plataforma == plataforma && m.MenuPadreId == null)
            .OrderBy(m => m.Orden)
            .ToListAsync();
    }

    public async Task<List<Menu>> GetHermanosAsync(int? padreId, Plataforma plataforma)
    {
        return await Context.Menus
            .Where(m => m.MenuPadreId == padreId && m.Plataforma == plataforma)
            .OrderBy(m => m.Orden)
            .ToListAsync();
    }

    public async Task<bool> TieneSubMenusAsync(int menuId)
        => await Context.Menus.AnyAsync(m => m.MenuPadreId == menuId);

    public async Task<List<Menu>> GetMenusVisiblesAsync(Plataforma plataforma, List<int> rolIds)
    {
        return await Context.Menus
            .Include(m => m.SubMenus
                .Where(s => !s.MenusRoles.Any() || s.MenusRoles.Any(mr => rolIds.Contains(mr.RolId)))
                .OrderBy(s => s.Orden))
                .ThenInclude(s => s.SubMenus
                    .Where(ss => !ss.MenusRoles.Any() || ss.MenusRoles.Any(mr => rolIds.Contains(mr.RolId)))
                    .OrderBy(ss => ss.Orden))
            .Where(m => m.Plataforma == plataforma)
            .Where(m => !m.MenusRoles.Any() || m.MenusRoles.Any(mr => rolIds.Contains(mr.RolId)))
            .Where(m => m.MenuPadreId == null)
            .OrderBy(m => m.Orden)
            .ToListAsync();
    }

    public async Task<List<int>> GetRolIdsAsync(int menuId)
    {
        return await Context.MenusRoles
            .Where(mr => mr.MenuId == menuId)
            .Select(mr => mr.RolId)
            .ToListAsync();
    }

    public async Task SincronizarRolesAsync(int menuId, List<int> rolIds)
    {
        var actuales = await Context.MenusRoles
            .Where(mr => mr.MenuId == menuId)
            .ToListAsync();

        var aEliminar = actuales.Where(mr => !rolIds.Contains(mr.RolId));
        Context.MenusRoles.RemoveRange(aEliminar);

        var existentes = actuales.Select(mr => mr.RolId).ToHashSet();
        var aCrear = rolIds.Where(id => !existentes.Contains(id))
            .Select(id => new MenuRol { MenuId = menuId, RolId = id });
        Context.MenusRoles.AddRange(aCrear);
    }

    public async Task<List<int>> GetUsuarioIdsPorRolesAsync(IEnumerable<int> rolIds)
    {
        return await Context.RolesUsuarios
            .Where(ru => rolIds.Contains(ru.RolId))
            .Select(ru => ru.UsuarioId)
            .Distinct()
            .ToListAsync();
    }
}
