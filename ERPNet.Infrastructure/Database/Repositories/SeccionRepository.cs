using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class SeccionRepository(ERPNetDbContext context) : Repository<Seccion>(context), ISeccionRepository
{
    public override async Task<List<Seccion>> GetAllAsync()
        => await Context.Set<Seccion>()
            .OrderBy(s => s.Nombre)
            .ToListAsync();
}
