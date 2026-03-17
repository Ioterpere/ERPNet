using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;

namespace ERPNet.Domain.Repositories;

public interface IMaquinariaRepository : IRepository<Maquinaria>
{
    Task<bool> ExisteCodigoAsync(string codigo, int? excludeId = null);
    Task<(List<Maquinaria> Items, int TotalRegistros)> GetPaginatedAsync(
        PaginacionFilter filtro, Alcance alcance, int seccionId);
}
