using ERPNet.Domain.Common;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public abstract class Repository<T>(ERPNetDbContext context) : IRepository<T> where T : BaseEntity
{
    protected ERPNetDbContext Context => context;

    public virtual async Task<T?> GetByIdAsync(int id)
        => await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);

    public virtual async Task<List<T>> GetAllAsync()
        => await context.Set<T>().ToListAsync();

    public virtual async Task<(List<T> Items, int TotalRegistros)> GetPaginatedAsync(PaginacionFilter filtro)
    {
        var query = context.Set<T>().AsQueryable();
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.Id)
            .Skip((filtro.Pagina - 1) * filtro.PorPagina)
            .Take(filtro.PorPagina)
            .ToListAsync();

        return (items, total);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await context.Set<T>().AddAsync(entity);
        return entity;
    }

    public virtual void Delete(T entity)
        => entity.IsDeleted = true;
}
