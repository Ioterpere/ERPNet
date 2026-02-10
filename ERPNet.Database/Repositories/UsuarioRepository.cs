using ERPNet.Domain.Repositories;
using ERPNet.Database.Context;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Database.Repositories;

public class UsuarioRepository(ERPNetDbContext context) : IUsuarioRepository
{
    public async Task<List<Usuario>> GetAllAsync()
    {
        return await context.Usuarios.ToListAsync();
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByIdConPermisosAsync(int id)
    {
        return await context.Usuarios
            .Include(u => u.Empleado)
            .Include(u => u.RolesUsuarios)
                .ThenInclude(ru => ru.Rol)
                    .ThenInclude(r => r.PermisosRolRecurso)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await context.Usuarios
            .Include(u => u.Empleado)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExisteEmailAsync(string email, int? excluirId = null)
    {
        return await context.Usuarios
            .AnyAsync(u => u.Email == email && (excluirId == null || u.Id != excluirId));
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        await context.Usuarios.AddAsync(usuario);
        return usuario;
    }

    public void Update(Usuario usuario)
    {
        context.Usuarios.Update(usuario);
    }

    public async Task UpdateUltimoAccesoAsync(int usuarioId, DateTime fecha)
    {
        await context.Usuarios
            .Where(u => u.Id == usuarioId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.UltimoAcceso, fecha));
    }
}
