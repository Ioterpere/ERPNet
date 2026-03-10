using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IMenuRepository: IRepository<Menu>
{
    Task<List<Menu>> GetMenusVisiblesAsync(Plataforma plataforma, List<int> rolIds);
    Task<List<Menu>> GetAllAdminAsync(Plataforma plataforma);
    Task<List<Menu>> GetHermanosAsync(int? padreId, Plataforma plataforma);
    Task<bool> TieneSubMenusAsync(int menuId);
    Task<List<int>> GetRolIdsAsync(int menuId);
    Task SincronizarRolesAsync(int menuId, List<int> rolIds);
    Task<List<int>> GetUsuarioIdsPorRolesAsync(IEnumerable<int> rolIds);
}
