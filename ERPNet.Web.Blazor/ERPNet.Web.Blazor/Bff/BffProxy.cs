using System.Net.Http.Headers;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// Proxy genérico BFF → API.
/// Captura cualquier petición a /api/** del WASM (autenticada via cookie),
/// añade el Bearer token del servidor y la reenvía a ERPNet.Api.
///
/// Type safety: garantizada en los extremos (WASM usa IEmpleadosClient, etc.;
/// API valida con sus controllers). El proxy solo reenvía bytes — sin boilerplate.
/// </summary>
public static class BffProxy
{
    public static IEndpointConventionBuilder MapBffProxy(this WebApplication app)
    {
        return app
            .Map("/api/{**rest}", Handle)
            .RequireAuthorization()
            .DisableAntiforgery();
    }

    private static async Task Handle(
        HttpContext ctx,
        BffTokenService tokenService,
        IHttpClientFactory factory)
    {
        var token = await tokenService.GetAccessTokenAsync();
        if (token is null)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using var apiRequest = new HttpRequestMessage
        {
            Method = new HttpMethod(ctx.Request.Method),
            RequestUri = new Uri(
                ctx.Request.Path.Value!.TrimStart('/') + ctx.Request.QueryString,
                UriKind.Relative)
        };

        apiRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (ctx.Request.ContentLength > 0 || ctx.Request.Headers.ContainsKey("Content-Type"))
        {
            apiRequest.Content = new StreamContent(ctx.Request.Body);
            if (ctx.Request.ContentType is not null)
                apiRequest.Content.Headers.TryAddWithoutValidation(
                    "Content-Type", ctx.Request.ContentType);
        }

        var client = factory.CreateClient("ErpNetApi");
        using var apiResponse = await client.SendAsync(
            apiRequest, HttpCompletionOption.ResponseHeadersRead);

        ctx.Response.StatusCode = (int)apiResponse.StatusCode;

        if (apiResponse.Content.Headers.ContentType is { } contentType)
            ctx.Response.ContentType = contentType.ToString();

        await apiResponse.Content.CopyToAsync(ctx.Response.Body);
    }
}
