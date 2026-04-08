using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface ICentroCosteRepository : IRepository<CentroCoste>
{
    Task<List<CentroCoste>> GetAllOrdenadosAsync();
}
