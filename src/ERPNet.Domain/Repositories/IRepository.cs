using ERPNet.Domain.Common;
using ERPNet.Domain.Filters;

namespace ERPNet.Domain.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<(List<T> Items, int TotalRegistros)> GetPaginatedAsync(PaginacionFilter filtro);
    Task<T> CreateAsync(T entity);
    void Delete(T entity);
}
