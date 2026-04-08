using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;

namespace ERPNet.Domain.Repositories;

public interface ICuentaRepository : IRepository<Cuenta>
{
    Task<bool> ExisteCodigoAsync(string codigo, int empresaId, int? excludeId = null);
    Task<(List<Cuenta> Items, int TotalRegistros)> GetPaginatedAsync(CuentaFilter filtro);
}
