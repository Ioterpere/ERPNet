using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IMenuRepository: IRepository<Menu>
{
    Task<List<Menu>> GetMenusVisiblesAsync(Plataforma plataforma, List<RecursoCodigo> codigosRecurso);
}
