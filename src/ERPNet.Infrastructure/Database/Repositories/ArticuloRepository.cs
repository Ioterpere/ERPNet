using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class ArticuloRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<Articulo>(context, currentUser), IArticuloRepository
{
    public override async Task<Articulo?> GetByIdAsync(int id)
    {
        return await Query
            .Include(a => a.FamiliaArticulo)
            .Include(a => a.TipoIva)
            .Include(a => a.FormatoArticulo)
            .Include(a => a.ConfiguracionCaducidad)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, int empresaId, int? excludeId = null)
    {
        return await Context.Articulos
            .AnyAsync(a => a.Codigo == codigo
                        && a.EmpresaId == empresaId
                        && (!excludeId.HasValue || a.Id != excludeId.Value));
    }

    private static readonly Dictionary<string, Func<IQueryable<Articulo>, bool, IOrderedQueryable<Articulo>>> _ordenadores = new()
    {
        ["Codigo"]                = (q, d) => d ? q.OrderByDescending(a => a.Codigo)              : q.OrderBy(a => a.Codigo),
        ["Descripcion"]           = (q, d) => d ? q.OrderByDescending(a => a.Descripcion)         : q.OrderBy(a => a.Descripcion),
        ["FamiliaArticuloNombre"] = (q, d) => d ? q.OrderByDescending(a => a.FamiliaArticulo!.Nombre) : q.OrderBy(a => a.FamiliaArticulo!.Nombre),
        ["PrecioCoste"]           = (q, d) => d ? q.OrderByDescending(a => a.PrecioCoste)         : q.OrderBy(a => a.PrecioCoste),
        ["PrecioVenta"]           = (q, d) => d ? q.OrderByDescending(a => a.PrecioVenta)         : q.OrderBy(a => a.PrecioVenta),
    };

    protected override IOrderedQueryable<Articulo> AplicarOrden(IQueryable<Articulo> query, string? campo, bool desc) =>
        campo is not null && _ordenadores.TryGetValue(campo, out var fn) ? fn(query, desc) : query.OrderBy(a => a.Codigo);

    public async Task<(List<Articulo> Items, int TotalRegistros)> GetPaginatedAsync(
        PaginacionFilter filtro, int empresaId)
    {
        var query = Context.Articulos
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId)
            .Include(a => a.FamiliaArticulo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var b = filtro.Busqueda.ToLower();
            query = query.Where(a => a.Codigo.ToLower().Contains(b)
                                  || a.Descripcion.ToLower().Contains(b));
        }

        var total = await query.CountAsync();
        var items = await AplicarOrden(query, filtro.OrdenarPor, filtro.OrdenDesc)
            .Skip(filtro.Pagina)
            .Take(filtro.PorPagina)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<ArticuloLog>> GetLogsAsync(int articuloId)
    {
        return await Context.ArticulosLog
            .Where(l => l.ArticuloId == articuloId)
            .Include(l => l.Usuario)
                .ThenInclude(u => u.Empleado)
            .OrderByDescending(l => l.Fecha)
            .ThenByDescending(l => l.Id)
            .ToListAsync();
    }
}
