using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class MenuRepository(ERPNetDbContext context) : Repository<Menu>(context), IMenuRepository
{
    public async Task<List<Menu>> GetMenusVisiblesAsync(Plataforma plataforma, List<RecursoCodigo> codigosRecurso)
    {
        var recursoIds = codigosRecurso.Select(c => (int)c).ToList();

        return await Context.Menus
            .Include(m => m.SubMenus.OrderBy(s => s.Orden))
            .Where(m => m.Plataforma == plataforma)
            .Where(m => m.RecursoId == null || recursoIds.Contains(m.RecursoId!.Value))
            .Where(m => m.MenuPadreId == null)
            .OrderBy(m => m.Orden)
            .ToListAsync();
    }
}
