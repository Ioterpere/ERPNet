using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface ITipoDiarioRepository : IRepository<TipoDiario>
{
    Task<List<TipoDiario>> GetAllOrdenadosAsync();
}
