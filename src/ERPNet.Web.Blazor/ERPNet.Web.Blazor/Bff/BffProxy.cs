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

        // Inyectar empresa activa como header para que la API filtre por empresa
        var empresaId = ctx.User.FindFirst("empresa_id")?.Value;
        if (empresaId is not null)
            apiRequest.Headers.TryAddWithoutValidation("X-Empresa-Id", empresaId);

        // Inyectar plataforma — este BFF siempre es WebBlazor
        apiRequest.Headers.TryAddWithoutValidation("X-Plataforma", "WebBlazor");

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

        if (apiResponse.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            // SSE: flush tras cada chunk para que los eventos lleguen en tiempo real
            var buffer = new byte[4096];
            var stream = await apiResponse.Content.ReadAsStreamAsync();
            int read;
            while ((read = await stream.ReadAsync(buffer, ctx.RequestAborted)) > 0)
            {
                await ctx.Response.Body.WriteAsync(buffer.AsMemory(0, read), ctx.RequestAborted);
                await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
            }
        }
        else
        {
            await apiResponse.Content.CopyToAsync(ctx.Response.Body);
        }
    }
}
