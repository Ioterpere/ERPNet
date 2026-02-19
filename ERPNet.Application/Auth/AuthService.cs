using ERPNet.Application.Auth.Interfaces;
using ERPNet.Contracts;
using ERPNet.Contracts.Auth;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using Microsoft.Extensions.Options;
using ERPNet.Application.Common.Interfaces;

namespace ERPNet.Application.Auth;

public class AuthService(
    IUsuarioRepository usuarioRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ILogIntentoLoginRepository logIntentoLoginRepository,
    ILogService logService,
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings,
    IOptions<LoginSettings> loginSettings) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly LoginSettings _loginSettings = loginSettings.Value;

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string ip)
    {
        // Verificar bloqueo por IP
        var ipDesde = DateTime.UtcNow.AddMinutes(-_loginSettings.IpWindowMinutes);
        var intentosFallidosIp = await logIntentoLoginRepository.CountRecentFailedByIpAsync(ip, ipDesde);
        if (intentosFallidosIp >= _loginSettings.MaxFailedAttemptsPerIp)
        {
            return Result<AuthResponse>.Failure(
                "Demasiados intentos de inicio de sesion desde esta direccion IP. Intente mas tarde.",
                ErrorType.Unauthorized);
        }

        // Buscar usuario
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email);

        if (usuario is not null)
        {
            // Verificar bloqueo por usuario
            var userDesde = DateTime.UtcNow.AddMinutes(-_loginSettings.LockoutMinutes);
            var intentosFallidosUser = await logIntentoLoginRepository.CountRecentFailedByEmailAsync(request.Email, userDesde);
            if (intentosFallidosUser >= _loginSettings.MaxFailedAttempts)
            {
                await RegistrarIntentoAsync(request.Email, ip, false, usuario.Id);
                return Result<AuthResponse>.Failure(
                    "Cuenta bloqueada temporalmente por demasiados intentos fallidos.",
                    ErrorType.Unauthorized);
            }

            // Validar password
            if (usuario.Activo && BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                await RegistrarIntentoAsync(request.Email, ip, true, usuario.Id);
                await usuarioRepository.UpdateUltimoAccesoAsync(usuario.Id, DateTime.UtcNow);
                await logService.EventAsync("Login", $"IP: {ip}", usuario.Id);

                var response = GenerarTokens(usuario, ContrasenaExpirada(usuario));
                await GuardarRefreshTokenAsync(response.RefreshToken, usuario.Id);

                return Result<AuthResponse>.Success(response);
            }

            // Password incorrecta
            await RegistrarIntentoAsync(request.Email, ip, false, usuario.Id);
        }
        else
        {
            // Usuario no encontrado — registrar intento sin UsuarioId
            await RegistrarIntentoAsync(request.Email, ip, false, null);
        }

        return Result<AuthResponse>.Failure(
            "Credenciales invalidas.",
            ErrorType.Unauthorized);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string token, string ip)
    {
        var tokenHash = tokenService.HashToken(token);
        var refreshToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (refreshToken is null)
        {
            return Result<AuthResponse>.Failure("Token invalido.", ErrorType.Unauthorized);
        }

        // Deteccion de reutilizacion: si ya esta revocado, revocar todos
        if (refreshToken.IsRevocado)
        {
            await refreshTokenRepository.RevokeAllByUsuarioIdAsync(refreshToken.UsuarioId);
            await unitOfWork.SaveChangesAsync();
            return Result<AuthResponse>.Failure(
                "Token reutilizado. Todos los tokens han sido revocados por seguridad.",
                ErrorType.Unauthorized);
        }

        if (refreshToken.IsExpirado)
        {
            return Result<AuthResponse>.Failure("Token expirado.", ErrorType.Unauthorized);
        }

        // Rotar: revocar actual y crear nuevo
        var usuario = refreshToken.Usuario;
        var response = GenerarTokens(usuario, ContrasenaExpirada(usuario));
        var nuevoHash = tokenService.HashToken(response.RefreshToken);

        refreshToken.FechaRevocacion = DateTime.UtcNow;
        refreshToken.ReemplazadoPor = nuevoHash;

        await GuardarRefreshTokenAsync(response.RefreshToken, usuario.Id);

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result> LogoutAsync(string token)
    {
        var tokenHash = tokenService.HashToken(token);
        var refreshToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (refreshToken is { IsActivo: true })
        {
            await logService.EventAsync("Logout", $"Logout manual", refreshToken.UsuarioId);
            refreshToken.FechaRevocacion = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();
        }

        // Idempotente: no falla si el token no existe o ya esta revocado
        return Result.Success();
    }

    private static bool ContrasenaExpirada(Usuario usuario)
        => usuario.CaducidadContrasena.HasValue && usuario.CaducidadContrasena.Value < DateTime.UtcNow;

    private AuthResponse GenerarTokens(Usuario usuario, bool requiereCambioContrasena)
    {
        var accessToken = tokenService.GenerateAccessToken(usuario);
        var refreshToken = tokenService.GenerateRefreshToken();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            RequiereCambioContrasena = requiereCambioContrasena
        };
    }

    private async Task GuardarRefreshTokenAsync(string rawToken, int usuarioId)
    {
        var hash = tokenService.HashToken(rawToken);
        var entity = new RefreshToken
        {
            Token = hash,
            UsuarioId = usuarioId,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(7)
        };

        await refreshTokenRepository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task RegistrarIntentoAsync(string email, string ip, bool exitoso, int? usuarioId)
    {
        var log = new LogIntentoLogin
        {
            NombreUsuario = email,
            DireccionIp = ip,
            FechaIntento = DateTime.UtcNow,
            Exitoso = exitoso,
            UsuarioId = usuarioId
        };

        await logIntentoLoginRepository.AddAsync(log);
        await unitOfWork.SaveChangesAsync();
    }
}
