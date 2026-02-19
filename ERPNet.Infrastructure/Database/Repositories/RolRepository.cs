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

    public async Task<IEnumerable<Recurso>> GetAllRecursosAsync()
    {
        return await Context.Recursos.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<PermisoRolRecurso>> GetPermisosAsync(int rolId)
    {
        return await Context.PermisosRolRecurso
            .Include(p => p.Recurso)
            .Where(p => p.RolId == rolId)
            .ToListAsync();
    }

    public async Task SincronizarPermisosAsync(int rolId, IEnumerable<PermisoRolRecurso> nuevos)
    {
        var actuales = await Context.PermisosRolRecurso
            .Where(p => p.RolId == rolId)
            .ToListAsync();

        Context.PermisosRolRecurso.RemoveRange(actuales);
        Context.PermisosRolRecurso.AddRange(nuevos);
    }

    public async Task SincronizarUsuariosAsync(int rolId, List<int> usuarioIds)
    {
        var actuales = await Context.RolesUsuarios
            .Where(ru => ru.RolId == rolId)
            .ToListAsync();

        var aEliminar = actuales.Where(ru => !usuarioIds.Contains(ru.UsuarioId));
        Context.RolesUsuarios.RemoveRange(aEliminar);

        var existentes = actuales.Select(ru => ru.UsuarioId).ToHashSet();
        var aCrear = usuarioIds.Where(id => !existentes.Contains(id))
            .Select(id => new RolUsuario { RolId = rolId, UsuarioId = id });
        Context.RolesUsuarios.AddRange(aCrear);
    }

    public async Task<IEnumerable<Usuario>> GetUsuariosAsync(int rolId)
    {
        return await Context.RolesUsuarios
            .Where(ru => ru.RolId == rolId)
            .Select(ru => ru.Usuario)
            .ToListAsync();
    }
}
