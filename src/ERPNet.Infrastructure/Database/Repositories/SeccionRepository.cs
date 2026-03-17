using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class SeccionRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<Seccion>(context, currentUser), ISeccionRepository
{
    public override async Task<List<Seccion>> GetAllAsync()
        => await Query.OrderBy(s => s.Nombre).ToListAsync();
}
