using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;

namespace ERPNet.Domain.Repositories;

public interface IArticuloRepository : IRepository<Articulo>
{
    Task<bool> ExisteCodigoAsync(string codigo, int empresaId, int? excludeId = null);
    Task<(List<Articulo> Items, int TotalRegistros)> GetPaginatedAsync(PaginacionFilter filtro, int empresaId);
    Task<List<ArticuloLog>> GetLogsAsync(int articuloId);
}
