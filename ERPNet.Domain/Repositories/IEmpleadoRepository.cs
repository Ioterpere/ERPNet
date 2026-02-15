using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;

namespace ERPNet.Domain.Repositories;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<bool> ExisteDniAsync(string dni, int? excludeId = null);
    Task<(List<Empleado> Items, int TotalRegistros)> GetPaginatedAsync(
        PaginacionFilter filtro, Alcance alcance, int empleadoId, int seccionId);
}
