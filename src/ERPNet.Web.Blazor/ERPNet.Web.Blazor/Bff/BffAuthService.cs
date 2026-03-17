using ERPNet.ApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// Gestiona el flujo de autenticación del BFF:
/// login (llama a ERPNet.Api via IAuthClient, guarda tokens en caché, crea cookie),
/// logout (limpia caché, cierra sesión) y sincronización de claims tras cambio de contraseña.
/// Devuelve tipos simples — las redirecciones las decide el controller.
/// </summary>
public sealed class BffAuthService(
    IAuthClient authClient,
    IDistributedCache cache,
    IHttpContextAccessor httpContextAccessor,
    IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Autentica al usuario contra ERPNet.Api, guarda los tokens en caché,
    /// auto-selecciona la primera empresa accesible y crea la cookie de sesión.
    /// Devuelve true si tuvo éxito.
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

        // Auto-selección de empresa: obtener la primera empresa accesible
        var (empresaId, empresaNombre) = await ObtenerPrimeraEmpresaAsync(loginResult.AccessToken);

        var sessionKey = Guid.NewGuid().ToString("N");
        await cache.SetStringAsync(
            $"bff-token:{sessionKey}",
            JsonSerializer.Serialize(new BffTokenData
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken,
                Expiration = loginResult.Expiration,
                EmpresaId = empresaId
            }),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
            });

        var claims = BuildClaims(userId, nombre, emailClaim, sessionKey,
            loginResult.RequiereCambioContrasena, empresaId, empresaNombre);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return true;
    }

    /// <summary>
    /// Fuerza un refresh del access token y re-firma la cookie con los claims actualizados.
    /// Preserva el EmpresaId activo en la sesión.
    /// </summary>
    public async Task<bool> SincronizarSesionAsync()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var sessionKey = httpContext.User.FindFirst("session_key")?.Value;
        if (sessionKey is null) return false;

        var tokenJson = await cache.GetStringAsync($"bff-token:{sessionKey}");
        if (tokenJson is null) return false;

        var tokenData = JsonSerializer.Deserialize<BffTokenData>(tokenJson);
        if (tokenData is null) return false;

        AuthResponse refreshResult;
        try
        {
            refreshResult = await authClient.RefreshAsync(
                new RefreshTokenRequest { RefreshToken = tokenData.RefreshToken });
        }
        catch { return false; }

        await cache.SetStringAsync(
            $"bff-token:{sessionKey}",
            JsonSerializer.Serialize(new BffTokenData
            {
                AccessToken = refreshResult.AccessToken,
                RefreshToken = refreshResult.RefreshToken,
                Expiration = refreshResult.Expiration,
                EmpresaId = tokenData.EmpresaId
            }),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
            });

        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var nombre = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        var emailClaim = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var empresaNombre = httpContext.User.FindFirst("empresa_nombre")?.Value;

        var claims = BuildClaims(userId, nombre, emailClaim, sessionKey,
            refreshResult.RequiereCambioContrasena, tokenData.EmpresaId, empresaNombre);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return true;
    }

    /// <summary>
    /// Cambia la empresa activa del usuario sin necesidad de logout.
    /// Verifica que la empresa sea accesible antes de aceptar el cambio.
    /// </summary>
    public async Task<bool> CambiarEmpresaAsync(int empresaId)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var sessionKey = httpContext.User.FindFirst("session_key")?.Value;
        if (sessionKey is null) return false;

        var tokenJson = await cache.GetStringAsync($"bff-token:{sessionKey}");
        if (tokenJson is null) return false;

        var tokenData = JsonSerializer.Deserialize<BffTokenData>(tokenJson);
        if (tokenData is null) return false;

        // Verificar que la empresa es accesible para el usuario
        var empresasAccesibles = await ObtenerEmpresasAsync(tokenData.AccessToken);
        var empresaDto = empresasAccesibles.FirstOrDefault(e => e.Id == empresaId);
        if (empresaDto is null) return false;

        tokenData.EmpresaId = empresaId;

        await cache.SetStringAsync(
            $"bff-token:{sessionKey}",
            JsonSerializer.Serialize(tokenData),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
            });

        // Re-firmar la cookie con el nuevo empresa_id y empresa_nombre
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var nombre = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        var emailClaim = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var requiereCambio = httpContext.User.FindFirst("requiere_cambio_contrasena")?.Value == "true";

        var claims = BuildClaims(userId, nombre, emailClaim, sessionKey, requiereCambio, empresaId, empresaDto.Nombre);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
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

    private async Task<(int? Id, string? Nombre)> ObtenerPrimeraEmpresaAsync(string accessToken)
    {
        var lista = await ObtenerEmpresasAsync(accessToken);
        var primera = lista.FirstOrDefault();
        return (primera?.Id, primera?.Nombre);
    }

    private async Task<List<EmpresaDto>> ObtenerEmpresasAsync(string accessToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ErpNetApi");
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/empresas/mis-empresas");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return [];

            return await response.Content.ReadFromJsonAsync<List<EmpresaDto>>(JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static List<Claim> BuildClaims(
        string userId, string nombre, string email,
        string sessionKey, bool requiereCambio, int? empresaId, string? empresaNombre = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, nombre),
            new(ClaimTypes.Email, email),
            new("session_key", sessionKey),
            new("requiere_cambio_contrasena", requiereCambio.ToString().ToLower())
        };

        if (empresaId.HasValue)
            claims.Add(new("empresa_id", empresaId.Value.ToString()));

        if (empresaNombre is not null)
            claims.Add(new("empresa_nombre", empresaNombre));

        return claims;
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

    private sealed class EmpresaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
