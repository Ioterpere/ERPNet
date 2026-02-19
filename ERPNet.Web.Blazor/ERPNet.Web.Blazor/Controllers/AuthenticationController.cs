using ERPNet.Web.Blazor.Bff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Web.Blazor.Controllers;

[Route("bff")]
public class AuthenticationController(BffAuthService authService) : Controller
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromForm] string? email,
        [FromForm] string? password,
        [FromForm] string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Redirect("/authentication/login?error=true");

        var ok = await authService.LoginAsync(email, password);
        return ok
            ? Redirect(SafeRedirect(returnUrl))
            : Redirect("/authentication/login?error=true");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl)
    {
        await authService.LogoutAsync();
        return Redirect(SafeRedirect(returnUrl));
    }

    /// <summary>
    /// Intenta refrescar el access token del usuario actual.
    /// El WASM lo llama cuando recibe un 401 para saber si puede reintentar.
    /// Devuelve 204 si el token está disponible (fresco o recién refrescado),
    /// 401 si la sesión ha expirado del todo y hay que hacer login.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromServices] BffTokenService tokenService)
    {
        var token = await tokenService.GetAccessTokenAsync();
        return token is not null ? NoContent() : Unauthorized();
    }

    /// <summary>
    /// Fuerza un refresh del access token y actualiza los claims de la cookie.
    /// Llamar desde WASM tras un cambio de contraseña exitoso para que
    /// el claim <c>requiere_cambio_contrasena</c> refleje el nuevo estado.
    /// </summary>
    [HttpPost("sincronizar-sesion")]
    public async Task<IActionResult> SincronizarSesion()
    {
        var ok = await authService.SincronizarSesionAsync();
        return ok ? NoContent() : Unauthorized();
    }

    /// <summary>
    /// Construye una URL de redirección segura que previene open redirects.
    /// </summary>
    private static string SafeRedirect(string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
            return "/";

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.PathAndQuery;

        if (returnUrl.StartsWith('/'))
            return returnUrl;

        return "/";
    }
}
