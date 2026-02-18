using ERPNet.Web.Blazor.Bff;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace ERPNet.Web.Blazor;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(string.Empty);

        // El formulario de Login.razor hace POST aquí con las credenciales.
        // Llama a ERPNet.Api, guarda los tokens en caché y crea la cookie de sesión.
        group.MapPost("/login", async (
            [FromForm] string? email,
            [FromForm] string? password,
            [FromForm] string? returnUrl,
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            IDistributedCache cache) =>
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Results.Redirect("/authentication/login?error=true");

            var client = httpClientFactory.CreateClient("ErpNetApi");
            HttpResponseMessage apiResponse;
            try
            {
                apiResponse = await client.PostAsJsonAsync("/api/auth/login",
                    new { email, password });
            }
            catch
            {
                return Results.Redirect("/authentication/login?error=true");
            }

            if (!apiResponse.IsSuccessStatusCode)
                return Results.Redirect("/authentication/login?error=true");

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var loginResult = await apiResponse.Content
                .ReadFromJsonAsync<ErpAuthResponse>(jsonOptions);

            if (loginResult?.AccessToken is null)
                return Results.Redirect("/authentication/login?error=true");

            // Extraer claims del payload JWT (sub, nombre, email)
            var payload = DecodeJwtPayload(loginResult.AccessToken);
            var userId = GetClaim(payload, "sub") ?? string.Empty;
            var nombre = GetClaim(payload, "nombre", "name", "unique_name") ?? email;
            var emailClaim = GetClaim(payload, "email") ?? email;

            // Guardar los tokens en el caché del servidor (nunca salen al navegador).
            // La expiración viene directamente del AuthResponse de la API.
            var sessionKey = Guid.NewGuid().ToString("N");
            var tokenData = JsonSerializer.Serialize(new BffTokenData
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken,
                Expiration = new DateTimeOffset(
                    DateTime.SpecifyKind(loginResult.Expiration, DateTimeKind.Utc))
            });

            await cache.SetStringAsync(
                $"bff-token:{sessionKey}",
                tokenData,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
                });

            // Crear la cookie de autenticación con los claims del usuario
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
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Results.Redirect(SafeRedirect(returnUrl));
        }).AllowAnonymous().DisableAntiforgery();

        // El formulario de LogInOrOut.razor hace POST aquí al cerrar sesión.
        group.MapPost("/logout", async (
            [FromForm] string? returnUrl,
            HttpContext httpContext,
            IDistributedCache cache) =>
        {
            var sessionKey = httpContext.User.FindFirst("session_key")?.Value;
            if (sessionKey is not null)
                await cache.RemoveAsync($"bff-token:{sessionKey}");

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect(SafeRedirect(returnUrl));
        });

        return group;
    }

    /// <summary>
    /// Construye una URL de redirección segura que previene open redirects.
    /// Acepta tanto URIs absolutas (https://host/path) como rutas relativas (/path).
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

    /// <summary>
    /// Decodifica el payload de un JWT sin validar la firma.
    /// Solo para extraer claims informativos (sub, nombre, email).
    /// </summary>
    private static JsonDocument? DecodeJwtPayload(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');

            var bytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonDocument.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetClaim(JsonDocument? doc, params string[] keys)
    {
        if (doc is null) return null;
        foreach (var key in keys)
        {
            if (doc.RootElement.TryGetProperty(key, out var prop))
                return prop.GetString();
        }
        return null;
    }
}
