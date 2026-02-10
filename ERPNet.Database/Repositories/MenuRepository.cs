using ERPNet.Application.Repositories;
using ERPNet.Database.Context;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Database.Repositories;

public class MenuRepository(ERPNetDbContext context) : IMenuRepository
{
    public async Task<List<Menu>> GetMenusVisiblesAsync(Plataforma plataforma, List<RecursoCodigo> codigosRecurso)
    {
        var recursoIds = codigosRecurso.Select(c => (int)c).ToList();

        return await context.Menus
            .Include(m => m.SubMenus.OrderBy(s => s.Orden))
            .Where(m => m.Plataforma == plataforma)
            .Where(m => m.RecursoId == null || recursoIds.Contains(m.RecursoId!.Value))
            .Where(m => m.MenuPadreId == null)
            .OrderBy(m => m.Orden)
            .ToListAsync();
    }

    public async Task<Menu?> GetByIdAsync(int id)
    {
        return await context.Menus.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Menu> CreateAsync(Menu menu)
    {
        context.Menus.Add(menu);
        await context.SaveChangesAsync();
        return menu;
    }
}
