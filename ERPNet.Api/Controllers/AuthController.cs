using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Contracts.Auth;
using ERPNet.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ERPNet.Api.Controllers;

public class AuthController(IAuthService authService) : BaseController
{
    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = GetIpAddress();
        var result = await authService.LoginAsync(request, ip);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var ip = GetIpAddress();
        var result = await authService.RefreshTokenAsync(request.RefreshToken, ip);
        return FromResult(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.LogoutAsync(request.RefreshToken);
        return FromResult(result);
    }

    private string GetIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";

    [SinPermiso]
    [AllowAnonymous]
    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new InvalidOperationException(
            "Excepcion de prueba",
            new ArgumentException("Inner exception de prueba"));
    }
}
