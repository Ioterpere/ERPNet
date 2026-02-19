using ERPNet.ApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// Gestiona el flujo de autenticación del BFF:
/// login (llama a ERPNet.Api via IAuthClient, guarda tokens en caché, crea cookie),
/// y logout (limpia caché, cierra sesión).
/// Devuelve tipos simples — las redirecciones las decide el controller.
/// </summary>
public sealed class BffAuthService(
    IAuthClient authClient,
    IDistributedCache cache,
    IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Autentica al usuario contra ERPNet.Api, guarda los tokens en caché
    /// y crea la cookie de sesión. Devuelve true si tuvo éxito.
    /// </summary>
    public async Task<bool> LoginAsync(string email, string password)
    {
        AuthResponse loginResult;
        try
        {
            loginResult = await authClient.LoginAsync(
                new LoginRequest { Email = email, Password = password });
        }
        catch
        {
            return false;
        }

        var payload = DecodeJwtPayload(loginResult.AccessToken);
        var userId = GetClaim(payload, "sub") ?? string.Empty;
        var nombre = GetClaim(payload, "nombre", "name", "unique_name") ?? email;
        var emailClaim = GetClaim(payload, "email") ?? email;

        var sessionKey = Guid.NewGuid().ToString("N");
        await cache.SetStringAsync(
            $"bff-token:{sessionKey}",
            JsonSerializer.Serialize(new BffTokenData
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken,
                Expiration = loginResult.Expiration
            }),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
            });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, nombre),
            new(ClaimTypes.Email, emailClaim),
            new("session_key", sessionKey),
            new("requiere_cambio_contrasena",
                loginResult.RequiereCambioContrasena.ToString().ToLower())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return true;
    }

    /// <summary>Elimina los tokens de caché y cierra la sesión.</summary>
    public async Task LogoutAsync()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var sessionKey = httpContext.User.FindFirst("session_key")?.Value;
        if (sessionKey is not null)
            await cache.RemoveAsync($"bff-token:{sessionKey}");

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private static JsonDocument? DecodeJwtPayload(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            return JsonDocument.Parse(
                System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
        }
        catch { return null; }
    }

    private static string? GetClaim(JsonDocument? doc, params string[] keys)
    {
        if (doc is null) return null;
        foreach (var key in keys)
            if (doc.RootElement.TryGetProperty(key, out var prop))
                return prop.GetString();
        return null;
    }
}
