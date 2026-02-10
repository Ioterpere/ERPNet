using ERPNet.Database.Context;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Database.Repositories;

public class LogRepository(ERPNetDbContext context) : ILogRepository
{
    public void Add(Log log)
    {
        context.Logs.Add(log);
    }

    public async Task<List<Log>> GetFilteredAsync(string? entidad, string? entidadId, int? usuarioId,
        string? accion, DateTime? desde, DateTime? hasta, int pagina, int porPagina)
    {
        var query = context.Logs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entidad))
            query = query.Where(l => l.Entidad == entidad);

        if (!string.IsNullOrWhiteSpace(entidadId))
            query = query.Where(l => l.EntidadId == entidadId);

        if (usuarioId.HasValue)
            query = query.Where(l => l.UsuarioId == usuarioId.Value);

        if (!string.IsNullOrWhiteSpace(accion))
            query = query.Where(l => l.Accion == accion);

        if (desde.HasValue)
            query = query.Where(l => l.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(l => l.Fecha <= hasta.Value);

        return await query
            .OrderByDescending(l => l.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToListAsync();
    }
}
