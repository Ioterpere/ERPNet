using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace ERPNet.Web.Blazor.Auth;

public interface ITokenCookieService
{
    void SetTokenCookie(HttpContext context, string accessToken, string refreshToken, DateTime expiry);
    (string AccessToken, string RefreshToken)? ReadTokens(HttpContext context);
    void ClearTokenCookie(HttpContext context);
    Task<string?> GetValidAccessTokenAsync(HttpContext context);
}

internal sealed class TokenCookieService : ITokenCookieService
{
    private readonly IDataProtector _protector;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BffSettings _settings;

    public TokenCookieService(
        IDataProtectionProvider dataProtection,
        IHttpClientFactory httpClientFactory,
        IOptions<BffSettings> settings)
    {
        _protector = dataProtection.CreateProtector("ERPNet.BFF.TokenCookie");
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    public void SetTokenCookie(HttpContext context, string accessToken, string refreshToken, DateTime expiry)
    {
        var tokenData = new TokenData(accessToken, refreshToken, expiry);
        var json = JsonSerializer.Serialize(tokenData);
        var encrypted = _protector.Protect(json);

        context.Response.Cookies.Append(_settings.CookieName, encrypted, BuildCookieOptions());
    }

    public (string AccessToken, string RefreshToken)? ReadTokens(HttpContext context)
    {
        var data = ReadTokenData(context);
        return data is null ? null : (data.AccessToken, data.RefreshToken);
    }

    private TokenData? ReadTokenData(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(_settings.CookieName, out var encrypted))
            return null;

        try
        {
            var json = _protector.Unprotect(encrypted);
            return JsonSerializer.Deserialize<TokenData>(json);
        }
        catch
        {
            return null;
        }
    }

    public void ClearTokenCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(_settings.CookieName, BuildCookieOptions());
    }

    public async Task<string?> GetValidAccessTokenAsync(HttpContext context)
    {
        var tokenData = ReadTokenData(context);
        if (tokenData is null)
            return null;

        var buffer = TimeSpan.FromMinutes(_settings.TokenRefreshBufferMinutes);
        if (tokenData.Expiry > DateTime.UtcNow.Add(buffer))
            return tokenData.AccessToken;

        // Token expired or about to expire — refresh
        return await RefreshAsync(context, tokenData);
    }

    private async Task<string?> RefreshAsync(HttpContext context, TokenData tokenData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("erpnet-api");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
            {
                Content = JsonContent.Create(new { refreshToken = tokenData.RefreshToken })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ClearTokenCookie(context);
                return null;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthRefreshResponse>();
            if (authResponse is null)
            {
                ClearTokenCookie(context);
                return null;
            }

            SetTokenCookie(context, authResponse.AccessToken, authResponse.RefreshToken, authResponse.Expiration);
            return authResponse.AccessToken;
        }
        catch
        {
            ClearTokenCookie(context);
            return null;
        }
    }

    private CookieOptions BuildCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/",
        MaxAge = TimeSpan.FromDays(7)
    };

    // Internal DTO to deserialize API auth responses
    private sealed class AuthRefreshResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}
