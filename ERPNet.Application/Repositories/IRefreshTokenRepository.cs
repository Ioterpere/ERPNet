using ERPNet.Domain.Entities;

namespace ERPNet.Application.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken token);
    Task RevokeAllByUsuarioIdAsync(int usuarioId);
    Task SaveChangesAsync();
}
