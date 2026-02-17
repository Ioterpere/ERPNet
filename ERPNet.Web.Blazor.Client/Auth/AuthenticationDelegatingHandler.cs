using System.Net;
using Microsoft.AspNetCore.Components;

namespace ERPNet.Web.Blazor.Client.Auth;

public class AuthenticationDelegatingHandler : DelegatingHandler
{
    // private readonly BffAuthenticationStateProvider _authStateProvider;
    // private readonly NavigationManager _navigationManager;

    // public AuthenticationDelegatingHandler(
    //     BffAuthenticationStateProvider authStateProvider,
    //     NavigationManager navigationManager)
    // {
    //     _authStateProvider = authStateProvider;
    //     _navigationManager = navigationManager;
    // }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // // Solo interceptar 401 de peticiones a la API
        // if (response.StatusCode == HttpStatusCode.Unauthorized &&
        //     request.RequestUri?.PathAndQuery.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) == true)
        // {
        //     _authStateProvider.NotifyLogout();
        //     // No redirigir aquí - dejar que AuthorizeView lo maneje
        // }

        return response;
    }
}
