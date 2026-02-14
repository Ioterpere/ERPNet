using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class MenuRepository(ERPNetDbContext context) : Repository<Menu>(context), IMenuRepository
{
    public async Task<List<Menu>> GetMenusVisiblesAsync(Plataforma plataforma, List<int> rolIds)
    {
        return await Context.Menus
            .Include(m => m.SubMenus
                .Where(s => !s.MenusRoles.Any() || s.MenusRoles.Any(mr => rolIds.Contains(mr.RolId)))
                .OrderBy(s => s.Orden))
            .Where(m => m.Plataforma == plataforma)
            .Where(m => !m.MenusRoles.Any() || m.MenusRoles.Any(mr => rolIds.Contains(mr.RolId)))
            .Where(m => m.MenuPadreId == null)
            .OrderBy(m => m.Orden)
            .ToListAsync();
    }
}
