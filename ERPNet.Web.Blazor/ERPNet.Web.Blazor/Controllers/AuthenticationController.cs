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
