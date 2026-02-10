using ERPNet.Application.Auth;

namespace ERPNet.Api.Services;

public class HttpUsuarioContextAccessor(IHttpContextAccessor httpContextAccessor) : IUsuarioContextAccessor
{
    public UsuarioContext? Current =>
        httpContextAccessor.HttpContext?.Items["UsuarioContext"] as UsuarioContext;
}
