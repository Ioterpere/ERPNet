using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class UsuarioRepository(ERPNetDbContext context, ICurrentUserProvider currentUser) : Repository<Usuario>(context, currentUser), IUsuarioRepository
{
    public async Task<Usuario?> GetByIdConPermisosAsync(int id)
    {
        return await Context.Usuarios
            .Include(u => u.Empleado)
            .Include(u => u.RolesUsuarios)
                .ThenInclude(ru => ru.Rol)
                    .ThenInclude(r => r.PermisosRolRecurso)
            .Include(u => u.UsuarioEmpresas.Where(ue => !ue.IsDeleted))
                .ThenInclude(ue => ue.Empresa)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await Context.Usuarios
            .Include(u => u.Empleado)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    private static readonly Dictionary<string, Func<IQueryable<Usuario>, bool, IOrderedQueryable<Usuario>>> _orden =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Email"]  = (q, d) => d ? q.OrderByDescending(u => (string)u.Email)
                                      : q.OrderBy(u => (string)u.Email),
            ["Nombre"] = (q, d) => d ? q.OrderByDescending(u => u.Empleado!.Nombre).ThenByDescending(u => u.Empleado!.Apellidos)
                                      : q.OrderBy(u => u.Empleado!.Nombre).ThenBy(u => u.Empleado!.Apellidos),
        };

    protected override IOrderedQueryable<Usuario> AplicarOrden(IQueryable<Usuario> query, string? campo, bool desc)
        => campo is not null && _orden.TryGetValue(campo, out var ordenar)
            ? ordenar(query, desc)
            : query.OrderByDescending(u => u.Id);

    public override async Task<(List<Usuario> Items, int TotalRegistros)> GetPaginatedAsync(PaginacionFilter filtro)
    {
        IQueryable<Usuario> query = Query.AsNoTracking().Include(u => u.Empleado);
        if (CurrentUser.Current?.EmpresaId is int empresaId)
            query = query.Where(u => u.UsuarioEmpresas.Any(ue => ue.EmpresaId == empresaId && !ue.IsDeleted));
        foreach (var termino in (filtro.Busqueda ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var t = termino;
            query = query.Where(u =>
                ((string)u.Email).Contains(t) ||
                u.Empleado.Nombre.Contains(t) ||
                u.Empleado.Apellidos.Contains(t));
        }
        var total = await query.CountAsync();
        var items = await AplicarOrden(query, filtro.OrdenarPor, filtro.OrdenDesc)
            .Skip(filtro.Pagina)
            .Take(filtro.PorPagina)
            .ToListAsync();
        return (items, total);
    }

    public async Task<bool> ExisteEmailAsync(string email, int? excluirId = null)
    {
        return await Context.Usuarios
            .AnyAsync(u => u.Email == email && (excluirId == null || u.Id != excluirId));
    }

    public async Task<bool> ExisteEmpleadoAsync(int empleadoId, int? excluirId = null)
    {
        return await Context.Usuarios
            .AnyAsync(u => u.EmpleadoId == empleadoId && (excluirId == null || u.Id != excluirId));
    }

    public async Task UpdateUltimoAccesoAsync(int usuarioId, DateTime fecha)
    {
        await Context.Usuarios
            .Where(u => u.Id == usuarioId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.UltimoAcceso, fecha));
    }

    public async Task<List<string>> GetEmailsByRolAsync(string nombreRol)
    {
        return await Context.Usuarios
            .Where(u => u.Activo && u.RolesUsuarios.Any(ru => ru.Rol.Nombre == nombreRol))
            .Select(u => (string)u.Email)
            .ToListAsync();
    }

    public async Task<List<(int RolId, int? EmpresaId)>> GetTodasAsignacionesRolAsync(int usuarioId)
    {
        var rows = await Context.RolesUsuarios
            .Where(ru => ru.UsuarioId == usuarioId)
            .Select(ru => new { ru.RolId, ru.EmpresaId })
            .ToListAsync();
        return rows.Select(r => (r.RolId, r.EmpresaId)).ToList();
    }

    public async Task SincronizarTodasAsignacionesRolAsync(int usuarioId, List<(int RolId, int? EmpresaId)> asignaciones)
    {
        var actuales = await Context.RolesUsuarios
            .Where(ru => ru.UsuarioId == usuarioId)
            .ToListAsync();
        Context.RolesUsuarios.RemoveRange(actuales);

        var nuevas = asignaciones.Select(a => new RolUsuario
        {
            UsuarioId = usuarioId,
            RolId = a.RolId,
            EmpresaId = a.EmpresaId
        });
        Context.RolesUsuarios.AddRange(nuevas);
    }

    public async Task<List<Rol>> GetRolesConNombreAsync(int usuarioId)
        => await Context.RolesUsuarios
            .Where(ru => ru.UsuarioId == usuarioId)
            .Select(ru => ru.Rol)
            .ToListAsync();

    public async Task SincronizarRolesAsync(int usuarioId, List<int> rolIds, int? empresaId = null)
    {
        var actuales = await Context.RolesUsuarios
            .Where(ru => ru.UsuarioId == usuarioId && ru.EmpresaId == empresaId)
            .ToListAsync();

        var aEliminar = actuales.Where(ru => !rolIds.Contains(ru.RolId));
        Context.RolesUsuarios.RemoveRange(aEliminar);

        var existentes = actuales.Select(ru => ru.RolId).ToHashSet();
        var aCrear = rolIds.Where(id => !existentes.Contains(id))
            .Select(id => new RolUsuario { UsuarioId = usuarioId, RolId = id, EmpresaId = empresaId });
        Context.RolesUsuarios.AddRange(aCrear);
    }
}
