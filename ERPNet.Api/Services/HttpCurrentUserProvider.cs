using ERPNet.Application.Auth;

namespace ERPNet.Api.Services;

public class HttpCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public UsuarioContext? Current =>
        httpContextAccessor.HttpContext?.Items["UsuarioContext"] as UsuarioContext;
}
