using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class RolRepository(ERPNetDbContext context) : Repository<Rol>(context), IRolRepository
{
    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        return await Context.Roles
            .AnyAsync(r => r.Nombre == nombre && (!excludeId.HasValue || r.Id != excludeId.Value));
    }

    public List<int> GetUsuarioIdsPorRol(int rolId)
    {
        return Context.RolesUsuarios
            .Where(ru => ru.RolId == rolId)
            .Select(ru => ru.UsuarioId)
            .ToList();
    }
}
