using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IMenuRepository
{
    Task<List<Menu>> GetMenusVisiblesAsync(Plataforma plataforma, List<RecursoCodigo> codigosRecurso);
    Task<Menu?> GetByIdAsync(int id);
    Task<Menu> CreateAsync(Menu menu);
}
