using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// Gestiona los tokens JWT almacenados en el caché del servidor.
/// Inyecta el token en peticiones, refresca si está próximo a caducar
/// e invalida la sesión si el refresh falla.
/// </summary>
public sealed class BffTokenService(
    IHttpClientFactory httpClientFactory,
    IDistributedCache cache,
    IHttpContextAccessor httpContextAccessor)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Devuelve el access token vigente para el usuario actual,
    /// refrescándolo de forma transparente si está a punto de caducar.
    /// Devuelve null si el usuario no está autenticado o la sesión ha expirado.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        var sessionKey = httpContextAccessor.HttpContext?.User.FindFirst("session_key")?.Value;
        if (sessionKey is null) return null;

        var tokenJson = await cache.GetStringAsync($"bff-token:{sessionKey}");
        if (tokenJson is null) return null;

        var tokenData = JsonSerializer.Deserialize<BffTokenData>(tokenJson);
        if (tokenData is null) return null;

        // Refrescar proactivamente si el token caduca en menos de 2 minutos.
        if (tokenData.Expiration <= DateTimeOffset.UtcNow.AddMinutes(2))
        {
            var client = httpClientFactory.CreateClient("ErpNetApi");
            tokenData = await RefreshAsync(sessionKey, tokenData, client);
            if (tokenData is null) return null;
        }

        return tokenData.AccessToken;
    }

    internal async Task<BffTokenData?> RefreshAsync(
        string sessionKey, BffTokenData current, HttpClient client)
    {
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/api/auth/refresh",
                new { refreshToken = current.RefreshToken });
        }
        catch
        {
            // Error de red o API caída: problema temporal, no cerrar sesión.
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            await InvalidateSessionAsync(sessionKey);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<ErpAuthResponse>(JsonOptions);
        if (result?.AccessToken is null)
        {
            await InvalidateSessionAsync(sessionKey);
            return null;
        }

        var updated = new BffTokenData
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            Expiration = new DateTimeOffset(
                DateTime.SpecifyKind(result.Expiration, DateTimeKind.Utc))
        };

        await cache.SetStringAsync(
            $"bff-token:{sessionKey}",
            JsonSerializer.Serialize(updated),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
            });

        return updated;
    }

    /// <summary>
    /// Elimina los tokens del caché y cierra la sesión del BFF.
    /// </summary>
    public async Task InvalidateSessionAsync(string sessionKey)
    {
        await cache.RemoveAsync($"bff-token:{sessionKey}");

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

/// <summary>
/// Tokens almacenados en el caché del servidor, nunca expuestos al navegador.
/// </summary>
public sealed class BffTokenData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset Expiration { get; set; }
}

/// <summary>
/// Mapea la respuesta AuthResponse de ERPNet.Api (login y refresh).
/// </summary>
internal sealed class ErpAuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public bool RequiereCambioContrasena { get; set; }
}
