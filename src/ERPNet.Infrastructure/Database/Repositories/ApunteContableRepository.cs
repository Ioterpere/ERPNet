using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class ApunteContableRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<ApunteContable>(context, currentUser), IApunteContableRepository
{
    public async Task<List<ApunteContable>> GetExtractoAsync(int cuentaId, ExtractoFilter filtro)
    {
        var query = Query
            .Include(a => a.TipoDiario)
            .Include(a => a.CentroCoste)
            .Where(a => a.CuentaId == cuentaId);

        if (filtro.TipoDiarioId.HasValue)
            query = query.Where(a => a.TipoDiarioId == filtro.TipoDiarioId.Value);

        if (filtro.CentroCosteId.HasValue)
            query = query.Where(a => a.CentroCosteId == filtro.CentroCosteId.Value);

        if (filtro.Desde.HasValue)
            query = query.Where(a => a.Fecha >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(a => a.Fecha <= filtro.Hasta.Value);

        if (filtro.EsDefinitivo.HasValue)
            query = query.Where(a => a.EsDefinitivo == filtro.EsDefinitivo.Value);

        if (filtro.Punteado.HasValue)
            query = filtro.Punteado.Value
                ? query.Where(a => a.IdPunteo != null)
                : query.Where(a => a.IdPunteo == null);

        return await query
            .OrderBy(a => a.Fecha)
            .ThenBy(a => a.Asiento)
            .ThenBy(a => a.NumLinea)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SaldoMensual>> GetSaldosMensualesAsync(int cuentaId, int anio)
    {
        return await Query
            .Where(a => a.CuentaId == cuentaId && a.Fecha.Year == anio)
            .GroupBy(a => a.Fecha.Month)
            .Select(g => new SaldoMensual(
                g.Key,
                g.Sum(a => a.Debe),
                g.Sum(a => a.Haber),
                g.Count()))
            .OrderBy(s => s.Mes)
            .ToListAsync();
    }
}
