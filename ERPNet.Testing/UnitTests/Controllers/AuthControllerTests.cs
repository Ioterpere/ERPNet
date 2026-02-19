using System.Net;
using ERPNet.Api.Controllers;
using ERPNet.Contracts.Auth;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common;
using ERPNet.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authService);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _sut.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
    }

    #region Login

    [Fact(DisplayName = "Login: exitoso devuelve 200")]
    public async Task Login_Exitoso_Devuelve200()
    {
        var response = new AuthResponse
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
        _authService.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<string>())
            .Returns(Result<AuthResponse>.Success(response));

        var result = await _sut.Login(new LoginRequest { Email = "test@test.com", Password = "pass" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("token", ((AuthResponse)okResult.Value!).AccessToken);
    }

    [Fact(DisplayName = "Login: fallido devuelve 401")]
    public async Task Login_Fallido_Devuelve401()
    {
        _authService.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<string>())
            .Returns(Result<AuthResponse>.Failure("Credenciales incorrectas", ErrorType.Unauthorized));

        var result = await _sut.Login(new LoginRequest { Email = "test@test.com", Password = "wrong" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Login: pasa IP al service")]
    public async Task Login_PasaIpAlService()
    {
        _authService.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<string>())
            .Returns(Result<AuthResponse>.Success(new AuthResponse
            {
                AccessToken = "t",
                RefreshToken = "r",
                Expiration = DateTime.UtcNow
            }));

        await _sut.Login(new LoginRequest { Email = "test@test.com", Password = "pass" });

        await _authService.Received(1).LoginAsync(Arg.Any<LoginRequest>(), "192.168.1.1");
    }

    #endregion

    #region Refresh

    [Fact(DisplayName = "Refresh: exitoso devuelve 200")]
    public async Task Refresh_Exitoso_Devuelve200()
    {
        var response = new AuthResponse
        {
            AccessToken = "new-token",
            RefreshToken = "new-refresh",
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
        _authService.RefreshTokenAsync("old-refresh", Arg.Any<string>())
            .Returns(Result<AuthResponse>.Success(response));

        var result = await _sut.Refresh(new RefreshTokenRequest { RefreshToken = "old-refresh" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("new-token", ((AuthResponse)okResult.Value!).AccessToken);
    }

    [Fact(DisplayName = "Refresh: token invalido devuelve 401")]
    public async Task Refresh_TokenInvalido_Devuelve401()
    {
        _authService.RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result<AuthResponse>.Failure("Token invalido", ErrorType.Unauthorized));

        var result = await _sut.Refresh(new RefreshTokenRequest { RefreshToken = "bad" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
    }

    #endregion

    #region Logout

    [Fact(DisplayName = "Logout: exitoso devuelve 204")]
    public async Task Logout_Exitoso_Devuelve204()
    {
        _authService.LogoutAsync("refresh-token").Returns(Result.Success());

        var result = await _sut.Logout(new RefreshTokenRequest { RefreshToken = "refresh-token" });

        Assert.IsType<NoContentResult>(result);
    }

    #endregion
}
