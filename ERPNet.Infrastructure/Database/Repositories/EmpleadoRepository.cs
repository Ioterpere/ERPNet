using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class EmpleadoRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<Empleado>(context, currentUser), IEmpleadoRepository
{
    public override async Task<Empleado?> GetByIdAsync(int id)
    {
        return await Query
            .Include(e => e.Foto)
            .Include(e => e.Seccion)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> ExisteDniAsync(string dni, int? excludeId = null)
    {
        var normalizado = dni.Trim().ToUpperInvariant();
        return await Context.Empleados
            .AnyAsync(e => e.DNI == normalizado && (!excludeId.HasValue || e.Id != excludeId.Value));
    }

    public async Task<(List<Empleado> Items, int TotalRegistros)> GetPaginatedAsync(
        PaginacionFilter filtro, Alcance alcance, int empleadoId, int seccionId)
    {
        var query = alcance switch
        {
            Alcance.Propio  => Query.Where(e => e.EncargadoId == empleadoId),
            Alcance.Seccion => Query.Where(e => e.SeccionId == seccionId),
            _               => Query
        };

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            query = query.Where(e =>
                (e.Nombre + " " + e.Apellidos).Contains(filtro.Busqueda) ||
                ((string)e.DNI).Contains(filtro.Busqueda));

        var total = await query.CountAsync();
        var items = await query
            .AsNoTracking()
            .OrderByDescending(e => e.Id)
            .Skip((filtro.Pagina - 1) * filtro.PorPagina)
            .Take(filtro.PorPagina)
            .ToListAsync();

        return (items, total);
    }
}
