using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ERPNet.Web.Blazor.Auth;

public class ServerAuthenticationStateProvider(
    IHttpContextAccessor httpContextAccessor,
    ITokenCookieService tokenCookieService) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
            return Task.FromResult(Anonymous());

        var tokens = tokenCookieService.ReadTokens(context);
        if (tokens is null)
            return Task.FromResult(Anonymous());

        // Cookie exists with valid tokens — user is authenticated for prerendering purposes
        var identity = new ClaimsIdentity("BFF");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}
