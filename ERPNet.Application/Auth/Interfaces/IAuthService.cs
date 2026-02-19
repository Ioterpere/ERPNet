using ERPNet.Contracts;
using ERPNet.Contracts.Auth;

namespace ERPNet.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string ip);
    Task<Result<AuthResponse>> RefreshTokenAsync(string token, string ip);
    Task<Result> LogoutAsync(string token);
}
