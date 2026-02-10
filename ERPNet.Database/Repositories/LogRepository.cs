using ERPNet.Database.Context;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Database.Repositories;

public class LogRepository(ERPNetDbContext context) : ILogRepository
{
    public void Add(Log log)
    {
        context.Logs.Add(log);
    }

    public async Task<List<Log>> GetFilteredAsync(LogFilter filter)
    {
        var query = context.Logs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Entidad))
            query = query.Where(l => l.Entidad == filter.Entidad);

        if (!string.IsNullOrWhiteSpace(filter.EntidadId))
            query = query.Where(l => l.EntidadId == filter.EntidadId);

        if (filter.UsuarioId.HasValue)
            query = query.Where(l => l.UsuarioId == filter.UsuarioId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Accion))
            query = query.Where(l => l.Accion == filter.Accion);

        if (filter.Desde.HasValue)
            query = query.Where(l => l.Fecha >= filter.Desde.Value);

        if (filter.Hasta.HasValue)
            query = query.Where(l => l.Fecha <= filter.Hasta.Value);

        return await query
            .OrderByDescending(l => l.Fecha)
            .Skip((filter.Pagina - 1) * filter.PorPagina)
            .Take(filter.PorPagina)
            .ToListAsync();
    }
}
