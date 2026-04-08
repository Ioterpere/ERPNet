using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class CuentaRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<Cuenta>(context, currentUser), ICuentaRepository
{
    public override async Task<Cuenta?> GetByIdAsync(int id)
    {
        return await Query
            .Include(c => c.CuentaPadre)
            .Include(c => c.CuentaAmortizacion)
            .Include(c => c.CuentaPagoDelegado)
            .Include(c => c.EmpresaVinculada)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, int empresaId, int? excludeId = null)
    {
        return await Context.Cuentas
            .AnyAsync(c => c.Codigo == codigo
                        && c.EmpresaId == empresaId
                        && (!excludeId.HasValue || c.Id != excludeId.Value));
    }

    protected override IOrderedQueryable<Cuenta> AplicarOrden(IQueryable<Cuenta> query, string? campo, bool desc) =>
        campo switch
        {
            "Descripcion" => desc ? query.OrderByDescending(c => c.Descripcion) : query.OrderBy(c => c.Descripcion),
            _             => query.OrderBy(c => c.Codigo),
        };

    public async Task<(List<Cuenta> Items, int TotalRegistros)> GetPaginatedAsync(CuentaFilter filtro)
    {
        var query = Query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var b = filtro.Busqueda.ToLower();
            query = query.Where(c => c.Descripcion.ToLower().Contains(b)
                                  || c.Codigo.ToLower().Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Codigo))
            query = query.Where(c => c.Codigo.StartsWith(filtro.Codigo));

        if (!string.IsNullOrWhiteSpace(filtro.Descripcion))
        {
            var desc = filtro.Descripcion.ToLower();
            query = query.Where(c => c.Descripcion.ToLower().Contains(desc));
        }

        if (filtro.ConNif == true)
            query = query.Where(c => c.Nif != null && c.Nif != string.Empty);
        else if (filtro.ConNif == false)
            query = query.Where(c => c.Nif == null || c.Nif == string.Empty);

        if (filtro.ConDescripcionSii == true)
            query = query.Where(c => c.DescripcionSII != null && c.DescripcionSII != string.Empty);
        else if (filtro.ConDescripcionSii == false)
            query = query.Where(c => c.DescripcionSII == null || c.DescripcionSII == string.Empty);

        if (filtro.ConNif != false && !string.IsNullOrWhiteSpace(filtro.Nif))
        {
            var nif = filtro.Nif.ToLower();
            query = query.Where(c => c.Nif != null && c.Nif.ToLower().Contains(nif));
        }

        // Filtros de apuntes (afectan qué cuentas se muestran)
        bool tieneFiltroDiario = filtro.TipoDiarioId.HasValue
            || filtro.CentroCosteId.HasValue
            || filtro.Desde.HasValue
            || filtro.Hasta.HasValue
            || filtro.EsDefinitivo.HasValue
            || filtro.Punteado.HasValue;

        if (tieneFiltroDiario)
        {
            query = query.Where(c => c.Apuntes.Any(a =>
                (!filtro.TipoDiarioId.HasValue  || a.TipoDiarioId == filtro.TipoDiarioId) &&
                (!filtro.CentroCosteId.HasValue || a.CentroCosteId == filtro.CentroCosteId) &&
                (!filtro.Desde.HasValue         || a.Fecha >= filtro.Desde.Value) &&
                (!filtro.Hasta.HasValue         || a.Fecha <= filtro.Hasta.Value) &&
                (!filtro.EsDefinitivo.HasValue  || a.EsDefinitivo == filtro.EsDefinitivo.Value) &&
                (!filtro.Punteado.HasValue      || (filtro.Punteado.Value ? a.IdPunteo != null : a.IdPunteo == null))
            ));
        }
        else if (filtro.ConSaldo == true || filtro.SoloConApuntes == true)
        {
            query = query.Where(c => c.Apuntes.Any());
        }

        var total = await query.CountAsync();
        var items = await AplicarOrden(query, filtro.OrdenarPor, filtro.OrdenDesc)
            .Skip(filtro.Pagina)
            .Take(filtro.PorPagina)
            .ToListAsync();

        return (items, total);
    }
}
