using ERPNet.Application.Auth;
using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Mailing;
using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Repositories;
using ERPNet.Application.Enums;
using ERPNet.Domain.Entities;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using ERPNet.Application.Interfaces;

namespace ERPNet.Testing.Tests;

public class AuthServiceTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly ILogIntentoLoginRepository _logIntentoRepo = Substitute.For<ILogIntentoLoginRepository>();
    private readonly ILogService _logService = Substitute.For<ILogService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();

    private readonly AuthService _sut;

    private const string TestIp = "127.0.0.1";
    private const string TestEmail = "admin@erpnet.com";
    private const string TestPassword = "Admin123!";

    public AuthServiceTests()
    {
        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "TESTING_KEY_AT_LEAST_32_CHARACTERS_LONG!!",
            Issuer = "Test",
            Audience = "Test",
            ExpirationMinutes = 30
        });

        var loginSettings = Options.Create(new LoginSettings
        {
            MaxFailedAttempts = 5,
            LockoutMinutes = 15,
            MaxFailedAttemptsPerIp = 20,
            IpWindowMinutes = 10
        });

        _tokenService.GenerateAccessToken(Arg.Any<Usuario>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");
        _tokenService.HashToken(Arg.Any<string>()).Returns("hashed-token");

        _sut = new AuthService(
            _usuarioRepo,
            _refreshTokenRepo,
            _logIntentoRepo,
            _logService,
            _unitOfWork,
            _tokenService,
            _emailService,
            jwtSettings,
            loginSettings);
    }

    private Usuario CrearUsuarioTest() => new()
    {
        Id = 1,
        Email = Email.From(TestEmail),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
        Activo = true,
        EmpleadoId = 1,
    };

    #region Login

    [Fact(DisplayName = "Login: credenciales validas devuelven tokens")]
    public async Task Login_CredencialesValidas_DevuelveTokens()
    {
        var usuario = CrearUsuarioTest();
        _usuarioRepo.GetByEmailAsync(TestEmail).Returns(usuario);

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = TestEmail, Password = TestPassword }, TestIp);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value!.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
    }

    [Fact(DisplayName = "Login: password incorrecta devuelve error")]
    public async Task Login_PasswordIncorrecta_DevuelveError()
    {
        var usuario = CrearUsuarioTest();
        _usuarioRepo.GetByEmailAsync(TestEmail).Returns(usuario);

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = TestEmail, Password = "WrongPassword" }, TestIp);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    [Fact(DisplayName = "Login: usuario inexistente devuelve error")]
    public async Task Login_UsuarioInexistente_DevuelveError()
    {
        _usuarioRepo.GetByEmailAsync("noexiste@test.com").Returns((Usuario?)null);

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = "noexiste@test.com", Password = TestPassword }, TestIp);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    [Fact(DisplayName = "Login: usuario inactivo devuelve error")]
    public async Task Login_UsuarioInactivo_DevuelveError()
    {
        var usuario = CrearUsuarioTest();
        usuario.Activo = false;
        _usuarioRepo.GetByEmailAsync(TestEmail).Returns(usuario);

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = TestEmail, Password = TestPassword }, TestIp);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    [Fact(DisplayName = "Login: cuenta bloqueada por intentos fallidos")]
    public async Task Login_CuentaBloqueada_DevuelveError()
    {
        var usuario = CrearUsuarioTest();
        _usuarioRepo.GetByEmailAsync(TestEmail).Returns(usuario);
        _logIntentoRepo.CountRecentFailedByEmailAsync(TestEmail, Arg.Any<DateTime>()).Returns(5);

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = TestEmail, Password = TestPassword }, TestIp);

        Assert.False(result.IsSuccess);
        Assert.Contains("bloqueada", result.Error!);
    }

    [Fact(DisplayName = "Login: IP bloqueada por demasiados intentos")]
    public async Task Login_IpBloqueada_DevuelveError()
    {
        _logIntentoRepo.CountRecentFailedByIpAsync(TestIp, Arg.Any<DateTime>()).Returns(20);

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = TestEmail, Password = TestPassword }, TestIp);

        Assert.False(result.IsSuccess);
        Assert.Contains("IP", result.Error!);
    }

    [Fact(DisplayName = "Login: registra intento exitoso")]
    public async Task Login_Exitoso_RegistraIntento()
    {
        var usuario = CrearUsuarioTest();
        _usuarioRepo.GetByEmailAsync(TestEmail).Returns(usuario);

        await _sut.LoginAsync(
            new LoginRequest { Email = TestEmail, Password = TestPassword }, TestIp);

        await _logIntentoRepo.Received(1).AddAsync(Arg.Is<LogIntentoLogin>(l => l.Exitoso));
    }

    [Fact(DisplayName = "Login: registra intento fallido")]
    public async Task Login_Fallido_RegistraIntento()
    {
        var usuario = CrearUsuarioTest();
        _usuarioRepo.GetByEmailAsync(TestEmail).Returns(usuario);

        await _sut.LoginAsync(
            new LoginRequest { Email = TestEmail, Password = "Wrong" }, TestIp);

        await _logIntentoRepo.Received(1).AddAsync(Arg.Is<LogIntentoLogin>(l => !l.Exitoso));
    }

    #endregion

    #region Refresh Token

    [Fact(DisplayName = "Refresh: token valido rota y devuelve nuevos tokens")]
    public async Task Refresh_TokenValido_RotaTokens()
    {
        var usuario = CrearUsuarioTest();
        var refreshToken = new RefreshToken
        {
            Token = "hashed-token",
            UsuarioId = 1,
            Usuario = usuario,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
        };
        _refreshTokenRepo.GetByTokenHashAsync("hashed-token").Returns(refreshToken);

        var result = await _sut.RefreshTokenAsync("raw-token", TestIp);

        Assert.True(result.IsSuccess);
        Assert.NotNull(refreshToken.FechaRevocacion);
        Assert.Equal("hashed-token", refreshToken.ReemplazadoPor);
    }

    [Fact(DisplayName = "Refresh: token inexistente devuelve error")]
    public async Task Refresh_TokenInexistente_DevuelveError()
    {
        _refreshTokenRepo.GetByTokenHashAsync(Arg.Any<string>()).Returns((RefreshToken?)null);

        var result = await _sut.RefreshTokenAsync("invalid-token", TestIp);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    [Fact(DisplayName = "Refresh: token expirado devuelve error")]
    public async Task Refresh_TokenExpirado_DevuelveError()
    {
        var refreshToken = new RefreshToken
        {
            Token = "hashed-token",
            UsuarioId = 1,
            Usuario = CrearUsuarioTest(),
            FechaCreacion = DateTime.UtcNow.AddDays(-8),
            FechaExpiracion = DateTime.UtcNow.AddDays(-1),
        };
        _refreshTokenRepo.GetByTokenHashAsync("hashed-token").Returns(refreshToken);

        var result = await _sut.RefreshTokenAsync("raw-token", TestIp);

        Assert.False(result.IsSuccess);
        Assert.Contains("expirado", result.Error!);
    }

    [Fact(DisplayName = "Refresh: token reutilizado revoca todos los tokens")]
    public async Task Refresh_TokenReutilizado_RevocaTodos()
    {
        var refreshToken = new RefreshToken
        {
            Token = "hashed-token",
            UsuarioId = 1,
            Usuario = CrearUsuarioTest(),
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaRevocacion = DateTime.UtcNow.AddMinutes(-5),
        };
        _refreshTokenRepo.GetByTokenHashAsync("hashed-token").Returns(refreshToken);

        var result = await _sut.RefreshTokenAsync("raw-token", TestIp);

        Assert.False(result.IsSuccess);
        Assert.Contains("reutilizado", result.Error!);
        await _refreshTokenRepo.Received(1).RevokeAllByUsuarioIdAsync(1);
    }

    #endregion

    #region Logout

    [Fact(DisplayName = "Logout: revoca token activo")]
    public async Task Logout_TokenActivo_LoRevoca()
    {
        var refreshToken = new RefreshToken
        {
            Token = "hashed-token",
            UsuarioId = 1,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
        };
        _refreshTokenRepo.GetByTokenHashAsync("hashed-token").Returns(refreshToken);

        var result = await _sut.LogoutAsync("raw-token");

        Assert.True(result.IsSuccess);
        Assert.NotNull(refreshToken.FechaRevocacion);
    }

    [Fact(DisplayName = "Logout: token inexistente no falla (idempotente)")]
    public async Task Logout_TokenInexistente_NoFalla()
    {
        _refreshTokenRepo.GetByTokenHashAsync(Arg.Any<string>()).Returns((RefreshToken?)null);

        var result = await _sut.LogoutAsync("whatever");

        Assert.True(result.IsSuccess);
    }

    #endregion
}
