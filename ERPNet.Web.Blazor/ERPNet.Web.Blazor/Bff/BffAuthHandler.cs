using System.Net.Http.Headers;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// DelegatingHandler que inyecta automáticamente el Bearer token del usuario actual
/// en las peticiones de los clientes tipados generados por NSwag.
/// Se registra via AddHttpMessageHandler en DependencyInjection.AddApiClients.
/// </summary>
public sealed class BffAuthHandler(BffTokenService tokenService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenService.GetAccessTokenAsync();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
