using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class UsuarioRepository(ERPNetDbContext context) : Repository<Usuario>(context), IUsuarioRepository
{
    public async Task<Usuario?> GetByIdConPermisosAsync(int id)
    {
        return await Context.Usuarios
            .Include(u => u.Empleado)
            .Include(u => u.RolesUsuarios)
                .ThenInclude(ru => ru.Rol)
                    .ThenInclude(r => r.PermisosRolRecurso)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await Context.Usuarios
            .Include(u => u.Empleado)
            .FirstOrDefaultAsync(u => u.Email == email);
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
}
