using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class TipoDiarioRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<TipoDiario>(context, currentUser), ITipoDiarioRepository
{
    public async Task<List<TipoDiario>> GetAllOrdenadosAsync()
    {
        return await Query
            .OrderBy(t => t.Codigo)
            .AsNoTracking()
            .ToListAsync();
    }
}
