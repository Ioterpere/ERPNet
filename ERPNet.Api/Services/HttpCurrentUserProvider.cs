using ERPNet.Application.Auth;
using ERPNet.Application.Auth.Interfaces;

namespace ERPNet.Api.Services;

public class HttpCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public UsuarioContext? Current =>
        httpContextAccessor.HttpContext?.Items["UsuarioContext"] as UsuarioContext;
}
