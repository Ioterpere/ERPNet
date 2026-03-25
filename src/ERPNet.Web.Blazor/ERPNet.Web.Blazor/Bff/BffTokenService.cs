using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
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

    // Margen de renovación anticipada: si el token expira en menos de este tiempo, se refresca.
    private const int MinutosUmbralRefresco = 2;

    // Un semáforo por sesión para que peticiones concurrentes no roten el mismo refresh token,
    // lo que haría que la API los invalide a todos por detección de reutilización.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _refreshLocks = new();

    private static string TokenCacheKey(string sessionKey) => $"bff-token:{sessionKey}";

    /// <summary>
    /// Devuelve el access token vigente para el usuario actual,
    /// refrescándolo de forma transparente si está a punto de caducar.
    /// Devuelve null si el usuario no está autenticado o la sesión ha expirado.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        var sessionKey = httpContextAccessor.HttpContext?.User.FindFirst("session_key")?.Value;
        if (sessionKey is null) return null;

        var tokenJson = await cache.GetStringAsync(TokenCacheKey(sessionKey));
        if (tokenJson is null) return null;

        var tokenData = JsonSerializer.Deserialize<BffTokenData>(tokenJson, JsonOptions);
        if (tokenData is null) return null;

        if (tokenData.Expiration <= DateTimeOffset.UtcNow.AddMinutes(MinutosUmbralRefresco))
        {
            var semaphore = _refreshLocks.GetOrAdd(sessionKey, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                // Doble comprobación: otra petición concurrente puede haber refrescado ya.
                var freshJson = await cache.GetStringAsync(TokenCacheKey(sessionKey));
                if (freshJson is not null)
                {
                    var fresh = JsonSerializer.Deserialize<BffTokenData>(freshJson, JsonOptions);
                    if (fresh?.Expiration > DateTimeOffset.UtcNow.AddMinutes(MinutosUmbralRefresco))
                        return fresh.AccessToken;
                    if (fresh is not null)
                        tokenData = fresh;
                }

                var client = httpClientFactory.CreateClient("ErpNetApi");
                tokenData = await RefreshAsync(sessionKey, tokenData, client);
                if (tokenData is null) return null;
            }
            finally
            {
                semaphore.Release();
            }
        }

        return tokenData.AccessToken;
    }

    /// <summary>
    /// Devuelve la empresa activa almacenada junto al token del usuario actual.
    /// </summary>
    public async Task<int?> GetEmpresaIdAsync()
    {
        var sessionKey = httpContextAccessor.HttpContext?.User.FindFirst("session_key")?.Value;
        if (sessionKey is null) return null;

        var tokenJson = await cache.GetStringAsync(TokenCacheKey(sessionKey));
        if (tokenJson is null) return null;

        var tokenData = JsonSerializer.Deserialize<BffTokenData>(tokenJson, JsonOptions);
        return tokenData?.EmpresaId;
    }

    /// <summary>
    /// Actualiza la empresa activa en el caché del token y en la cookie de sesión.
    /// </summary>
    public async Task<bool> SetEmpresaIdAsync(int empresaId)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var sessionKey = httpContext?.User.FindFirst("session_key")?.Value;
        if (sessionKey is null) return false;

        var tokenJson = await cache.GetStringAsync(TokenCacheKey(sessionKey));
        if (tokenJson is null) return false;

        var tokenData = JsonSerializer.Deserialize<BffTokenData>(tokenJson, JsonOptions);
        if (tokenData is null) return false;

        tokenData.EmpresaId = empresaId;

        await cache.SetStringAsync(
            TokenCacheKey(sessionKey),
            JsonSerializer.Serialize(tokenData),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
            });

        return true;
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
                DateTime.SpecifyKind(result.Expiration, DateTimeKind.Utc)),
            EmpresaId = current.EmpresaId
        };

        await cache.SetStringAsync(
            TokenCacheKey(sessionKey),
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
        await cache.RemoveAsync(TokenCacheKey(sessionKey));

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
    public int? EmpresaId { get; set; }
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
