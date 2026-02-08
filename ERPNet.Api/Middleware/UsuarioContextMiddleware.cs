using System.Security.Claims;
using ERPNet.Application.Auth;
using ERPNet.Application.Interfaces;
using ERPNet.Application.Repositories;

namespace ERPNet.Api.Middleware;

public class UsuarioContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ICacheService cache,
        IUsuarioRepository usuarioRepository)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is not null && int.TryParse(userId, out var id))
        {
            var cacheKey = $"usuario:{id}";
            var usuarioContext = cache.Get<UsuarioContext>(cacheKey);

            if (usuarioContext is null)
            {
                var usuario = await usuarioRepository.GetByIdAsync(id);

                if (usuario is not null)
                {
                    usuarioContext = new UsuarioContext(usuario.Id, usuario.Email, usuario.EmpleadoId);
                    cache.Set(cacheKey, usuarioContext);
                }
            }

            if (usuarioContext is not null)
            {
                context.Items["UsuarioContext"] = usuarioContext;
            }
        }

        await next(context);
    }
}
