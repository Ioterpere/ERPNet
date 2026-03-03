using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class RolRepository(ERPNetDbContext context, ICurrentUserProvider currentUser) : Repository<Rol>(context, currentUser), IRolRepository
{
    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        return await Context.Roles
            .AnyAsync(r => r.Nombre == nombre && (!excludeId.HasValue || r.Id != excludeId.Value));
    }

    public async Task<List<int>> GetUsuarioIdsPorRolAsync(int rolId)
    {
        return await Context.RolesUsuarios
            .Where(ru => ru.RolId == rolId)
            .Select(ru => ru.UsuarioId)
            .ToListAsync();
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

    public async Task<List<(int UsuarioId, int? EmpresaId)>> GetTodasAsignacionesUsuarioAsync(int rolId)
    {
        var rows = await Context.RolesUsuarios
            .Where(ru => ru.RolId == rolId)
            .Select(ru => new { ru.UsuarioId, ru.EmpresaId })
            .ToListAsync();
        return rows.Select(r => (r.UsuarioId, r.EmpresaId)).ToList();
    }

    public async Task SincronizarTodasAsignacionesUsuarioAsync(int rolId, List<(int UsuarioId, int? EmpresaId)> asignaciones)
    {
        var actuales = await Context.RolesUsuarios
            .Where(ru => ru.RolId == rolId)
            .ToListAsync();
        Context.RolesUsuarios.RemoveRange(actuales);

        var nuevas = asignaciones.Select(a => new RolUsuario
        {
            RolId = rolId,
            UsuarioId = a.UsuarioId,
            EmpresaId = a.EmpresaId
        });
        Context.RolesUsuarios.AddRange(nuevas);
    }
}
