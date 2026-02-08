using ERPNet.Domain.Entities;

namespace ERPNet.Application.Auth.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Usuario usuario);
    string GenerateRefreshToken();
    string HashToken(string token);
}
