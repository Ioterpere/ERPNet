using ERPNet.Domain.Repositories;
using ERPNet.Database.Context;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Database.Repositories;

public class RefreshTokenRepository(ERPNetDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await context.RefreshTokens
            .Include(rt => rt.Usuario)
                .ThenInclude(u => u.Empleado)
            .FirstOrDefaultAsync(rt => rt.Token == tokenHash);
    }

    public async Task AddAsync(RefreshToken token)
    {
        await context.RefreshTokens.AddAsync(token);
    }

    public async Task RevokeAllByUsuarioIdAsync(int usuarioId)
    {
        await context.RefreshTokens
            .Where(rt => rt.UsuarioId == usuarioId && rt.FechaRevocacion == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.FechaRevocacion, DateTime.UtcNow));
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
