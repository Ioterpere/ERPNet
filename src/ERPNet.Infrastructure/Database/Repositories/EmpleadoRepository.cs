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

    private static readonly Dictionary<string, Func<IQueryable<Empleado>, bool, IOrderedQueryable<Empleado>>> _orden =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Nombre"]       = (q, d) => d ? q.OrderByDescending(e => e.Nombre).ThenByDescending(e => e.Apellidos)
                                            : q.OrderBy(e => e.Nombre).ThenBy(e => e.Apellidos),
            ["Dni"]          = (q, d) => d ? q.OrderByDescending(e => (string)e.DNI)
                                            : q.OrderBy(e => (string)e.DNI),
            ["SeccionNombre"] = (q, d) => d ? q.OrderByDescending(e => e.Seccion!.Nombre)
                                             : q.OrderBy(e => e.Seccion!.Nombre),
        };

    protected override IOrderedQueryable<Empleado> AplicarOrden(IQueryable<Empleado> query, string? campo, bool desc)
        => campo is not null && _orden.TryGetValue(campo, out var ordenar)
            ? ordenar(query, desc)
            : _orden["Nombre"](query, desc);

    public async Task<(List<Empleado> Items, int TotalRegistros)> GetPaginatedAsync(
        EmpleadoFilter filtro, Alcance alcance, int empleadoId, int seccionId)
    {
        var query = alcance switch
        {
            Alcance.Propio  => Query.Where(e => e.EncargadoId == empleadoId),
            Alcance.Seccion => Query.Where(e => e.SeccionId == seccionId),
            _               => Query
        };

        foreach (var termino in (filtro.Busqueda ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var t = termino;
            query = query.Where(e =>
                (e.Nombre + " " + e.Apellidos).Contains(t) ||
                ((string)e.DNI).Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
            query = query.Where(e => e.Nombre.Contains(filtro.Nombre));

        if (!string.IsNullOrWhiteSpace(filtro.Apellidos))
            query = query.Where(e => e.Apellidos.Contains(filtro.Apellidos));

        if (filtro.Activo.HasValue)
            query = query.Where(e => e.Activo == filtro.Activo.Value);

        if (filtro.SeccionId.HasValue)
            query = query.Where(e => e.SeccionId == filtro.SeccionId.Value);

        if (filtro.FechaAltaDesde.HasValue)
            query = query.Where(e => e.CreatedAt >= filtro.FechaAltaDesde.Value.ToDateTime(TimeOnly.MinValue));

        if (filtro.FechaAltaHasta.HasValue)
            query = query.Where(e => e.CreatedAt <= filtro.FechaAltaHasta.Value.ToDateTime(TimeOnly.MaxValue));

        var total = await query.CountAsync();
        var items = await AplicarOrden(query, filtro.OrdenarPor, filtro.OrdenDesc)
            .Include(e => e.Seccion)
            .AsNoTracking()
            .Skip(filtro.Pagina)
            .Take(filtro.PorPagina)
            .ToListAsync();

        return (items, total);
    }
}
