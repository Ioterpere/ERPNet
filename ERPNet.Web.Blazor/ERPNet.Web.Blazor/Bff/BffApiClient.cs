using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// Cliente HTTP que inyecta automáticamente el JWT del usuario actual
/// como Bearer token en las peticiones a ERPNet.Api.
/// Si el token está a punto de caducar lo refresca de forma transparente
/// usando el RefreshToken almacenado en caché.
/// </summary>
public sealed class BffApiClient(
    IHttpClientFactory httpClientFactory,
    IDistributedCache cache,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<HttpResponseMessage> GetAsync(string relativeUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        return await SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string relativeUrl, T body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
        {
            Content = JsonContent.Create(body)
        };
        return await SendAsync(request);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string relativeUrl, T body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, relativeUrl)
        {
            Content = JsonContent.Create(body)
        };
        return await SendAsync(request);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string relativeUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, relativeUrl);
        return await SendAsync(request);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var client = httpClientFactory.CreateClient("ErpNetApi");

        var sessionKey = httpContextAccessor.HttpContext?.User.FindFirst("session_key")?.Value;
        if (sessionKey is not null)
        {
            var tokenJson = await cache.GetStringAsync($"bff-token:{sessionKey}");
            if (tokenJson is null)
            {
                // Token no encontrado en caché (reinicio del servidor, expiración anticipada).
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            var tokenData = JsonSerializer.Deserialize<BffTokenData>(tokenJson);
            if (tokenData is null)
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);

            // Refrescar proactivamente si el token caduca en menos de 2 minutos
            if (tokenData.Expiration <= DateTimeOffset.UtcNow.AddMinutes(2))
            {
                tokenData = await RefreshAsync(sessionKey, tokenData, client);
                if (tokenData is null)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
        }

        return await client.SendAsync(request);
    }

    private async Task<BffTokenData?> RefreshAsync(string sessionKey, BffTokenData current, HttpClient client)
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
            // En la próxima petición se volverá a intentar el refresh.
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            // El refresh token ha caducado o es inválido.
            // Limpiar la sesión para que el usuario vuelva al login.
            await InvalidateSessionAsync(sessionKey);
            return null;
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = await response.Content.ReadFromJsonAsync<ErpAuthResponse>(jsonOptions);

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
    /// La próxima petición a un recurso protegido redirigirá al login.
    /// </summary>
    private async Task InvalidateSessionAsync(string sessionKey)
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
