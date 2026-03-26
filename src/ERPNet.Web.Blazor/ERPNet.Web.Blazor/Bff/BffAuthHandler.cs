using System.Net.Http.Headers;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// DelegatingHandler que inyecta automáticamente el Bearer token del usuario actual
/// en las peticiones de los clientes tipados generados por NSwag.
/// Se registra via AddHttpMessageHandler en DependencyInjection.AddApiClients.
/// </summary>
public sealed class BffAuthHandler(
    BffTokenService tokenService,
    IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenService.GetAccessTokenAsync();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var empresaId = httpContext.User.FindFirst("empresa_id")?.Value;
            if (empresaId is not null)
                request.Headers.TryAddWithoutValidation("X-Empresa-Id", empresaId);
            request.Headers.TryAddWithoutValidation("X-Plataforma", "WebBlazor");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
