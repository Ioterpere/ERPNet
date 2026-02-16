using System.Net.Http.Json;
using System.Security.Claims;
using ERPNet.Web.Blazor.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace ERPNet.Web.Blazor.Client.Auth;

public class BffAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;
    private UsuarioInfo? _cachedUser;

    public BffAuthenticationStateProvider(HttpClient http)
    {
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var usuario = await _http.GetFromJsonAsync<UsuarioInfo>("/bff/me");

            if (usuario is null)
                return Anonymous();

            _cachedUser = usuario;
            return new AuthenticationState(CreatePrincipal(usuario));
        }
        catch
        {
            _cachedUser = null;
            return Anonymous();
        }
    }

    public void NotifyLogin(UsuarioInfo usuario)
    {
        _cachedUser = usuario;
        var state = Task.FromResult(new AuthenticationState(CreatePrincipal(usuario)));
        NotifyAuthenticationStateChanged(state);
    }

    public void NotifyLogout()
    {
        _cachedUser = null;
        var state = Task.FromResult(Anonymous());
        NotifyAuthenticationStateChanged(state);
    }

    public UsuarioInfo? CachedUser => _cachedUser;

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static ClaimsPrincipal CreatePrincipal(UsuarioInfo usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new("EmpleadoId", usuario.EmpleadoId.ToString()),
            new("SeccionId", usuario.SeccionId.ToString())
        };

        foreach (var rolId in usuario.Roles)
            claims.Add(new Claim(ClaimTypes.Role, rolId.ToString()));

        var identity = new ClaimsIdentity(claims, "BFF");
        return new ClaimsPrincipal(identity);
    }
}
