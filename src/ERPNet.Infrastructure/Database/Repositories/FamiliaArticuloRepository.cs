using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class FamiliaArticuloRepository(ERPNetDbContext context, ICurrentUserProvider currentUser)
    : Repository<FamiliaArticulo>(context, currentUser), IFamiliaArticuloRepository
{
    public async Task<List<FamiliaArticulo>> GetAllByEmpresaAsync(int empresaId)
    {
        return await Context.FamiliasArticulo
            .AsNoTracking()
            .Where(f => f.EmpresaId == empresaId)
            .Include(f => f.FamiliaPadre)
            .OrderBy(f => f.Nombre)
            .ToListAsync();
    }
}
