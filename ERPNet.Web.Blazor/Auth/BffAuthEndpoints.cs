using System.Net.Http.Headers;
using ERPNet.Web.Blazor.Shared.Auth;

namespace ERPNet.Web.Blazor.Auth;

public static class BffAuthEndpoints
{
    public static WebApplication MapBffAuthEndpoints(this WebApplication app)
    {
        var bff = app.MapGroup("/bff");

        bff.MapPost("/login", LoginAsync);
        bff.MapPost("/logout", LogoutAsync);
        bff.MapGet("/me", MeAsync);

        return app;
    }

    private static async Task<IResult> LoginAsync(
        BffLoginRequest request,
        IHttpClientFactory httpClientFactory,
        ITokenCookieService tokenCookieService,
        HttpContext context)
    {
        var client = httpClientFactory.CreateClient("erpnet-api");

        var apiRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { request.Email, request.Password })
        };

        // Forward client IP for rate limiting
        var clientIp = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        if (clientIp is not null)
            apiRequest.Headers.Add("X-Forwarded-For", clientIp);

        var response = await client.SendAsync(apiRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            return Results.Text(errorBody, response.Content.Headers.ContentType?.ToString() ?? "application/json", statusCode: (int)response.StatusCode);
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthLoginResponse>();
        if (authResponse is null)
            return Results.Problem("Respuesta inesperada de la API", statusCode: 502);

        tokenCookieService.SetTokenCookie(context, authResponse.AccessToken, authResponse.RefreshToken, authResponse.Expiration);

        return Results.Ok(new BffLoginResponse
        {
            RequiereCambioContrasena = authResponse.RequiereCambioContrasena
        });
    }

    private static async Task<IResult> LogoutAsync(
        IHttpClientFactory httpClientFactory,
        ITokenCookieService tokenCookieService,
        HttpContext context)
    {
        var tokens = tokenCookieService.ReadTokens(context);

        if (tokens is not null)
        {
            try
            {
                var client = httpClientFactory.CreateClient("erpnet-api");
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
                {
                    Content = JsonContent.Create(new { refreshToken = tokens.Value.RefreshToken })
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Value.AccessToken);

                await client.SendAsync(request);
            }
            catch
            {
                // Logout should be idempotent — clear cookie regardless
            }
        }

        tokenCookieService.ClearTokenCookie(context);
        return Results.Ok();
    }

    private static async Task<IResult> MeAsync(
        IHttpClientFactory httpClientFactory,
        ITokenCookieService tokenCookieService,
        HttpContext context)
    {
        var accessToken = await tokenCookieService.GetValidAccessTokenAsync(context);
        if (accessToken is null)
            return Results.Unauthorized();

        var client = httpClientFactory.CreateClient("erpnet-api");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/usuarios/account");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return Results.Unauthorized();

        var usuario = await response.Content.ReadFromJsonAsync<UsuarioInfo>();
        return usuario is not null ? Results.Ok(usuario) : Results.Unauthorized();
    }

    // Internal DTO to deserialize API login response (includes tokens)
    private sealed class AuthLoginResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public bool RequiereCambioContrasena { get; set; }
    }
}
