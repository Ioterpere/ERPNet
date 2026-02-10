using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken token);
    Task RevokeAllByUsuarioIdAsync(int usuarioId);
}
