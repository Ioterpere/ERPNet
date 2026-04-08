using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class CentroCosteRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<CentroCoste>(context, currentUser), ICentroCosteRepository
{
    public async Task<List<CentroCoste>> GetAllOrdenadosAsync()
    {
        return await Query
            .OrderBy(c => c.Codigo)
            .AsNoTracking()
            .ToListAsync();
    }
}
