using System.Linq.Expressions;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Common;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public abstract class Repository<T>(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : IRepository<T> where T : BaseEntity
{
    protected ERPNetDbContext Context => context;
    protected DbSet<T> DbSet => context.Set<T>();
    protected ICurrentUserProvider CurrentUser => currentUser;

    /// <summary>
    /// IQueryable base con el filtro de empresa aplicado automáticamente
    /// para entidades que implementen IPerteneceEmpresa.
    /// </summary>
    protected IQueryable<T> Query
    {
        get
        {
            if (typeof(IPerteneceEmpresa).IsAssignableFrom(typeof(T))
                && currentUser.Current?.EmpresaId is int empresaId)
                return DbSet.Where(e => ((IPerteneceEmpresa)e).EmpresaId == empresaId);
            return DbSet;
        }
    }

    public virtual async Task<T?> GetByIdAsync(int id)
        => await Query.FirstOrDefaultAsync(e => e.Id == id);

    public virtual async Task<List<T>> GetAllAsync()
        => await Query.AsNoTracking().ToListAsync();

    protected virtual Expression<Func<T, bool>>? GetBusquedaPredicate(string busqueda) => null;

    protected virtual IOrderedQueryable<T> AplicarOrden(IQueryable<T> query, string? campo, bool desc)
        => query.OrderByDescending(e => e.Id);

    public virtual async Task<(List<T> Items, int TotalRegistros)> GetPaginatedAsync(PaginacionFilter filtro)
    {
        var query = Query.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            foreach (var termino in filtro.Busqueda.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (GetBusquedaPredicate(termino) is { } pred)
                    query = query.Where(pred);
        var total = await query.CountAsync();
        var items = await AplicarOrden(query, filtro.OrdenarPor, filtro.OrdenDesc)
            .Skip(filtro.Pagina)
            .Take(filtro.PorPagina)
            .ToListAsync();

        return (items, total);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public virtual void Delete(T entity)
        => entity.IsDeleted = true;
}
