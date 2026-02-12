using ERPNet.Domain.Common;

namespace ERPNet.Domain.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
