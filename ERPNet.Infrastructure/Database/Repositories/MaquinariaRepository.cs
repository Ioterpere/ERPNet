using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class MaquinariaRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<Maquinaria>(context, currentUser), IMaquinariaRepository
{
    public override async Task<Maquinaria?> GetByIdAsync(int id)
    {
        return await Query
            .Include(m => m.FichaTecnica)
            .Include(m => m.Manual)
            .Include(m => m.CertificadoCe)
            .Include(m => m.Foto)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, int? excludeId = null)
    {
        return await Context.Maquinas
            .AnyAsync(m => m.Codigo == codigo && (!excludeId.HasValue || m.Id != excludeId.Value));
    }

    public async Task<(List<Maquinaria> Items, int TotalRegistros)> GetPaginatedAsync(
        PaginacionFilter filtro, Alcance alcance, int seccionId)
    {
        var query = alcance switch
        {
            Alcance.Seccion => Query.Where(m => m.SeccionId == seccionId),
            Alcance.Propio  => Query.Where(_ => false),
            _               => Query
        };

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.Id)
            .Skip((filtro.Pagina - 1) * filtro.PorPagina)
            .Take(filtro.PorPagina)
            .ToListAsync();

        return (items, total);
    }
}
