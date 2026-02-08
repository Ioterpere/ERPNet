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
                var usuario = await usuarioRepository.GetByIdConPermisosAsync(id);

                if (usuario is not null)
                {
                    var permisos = usuario.RolesUsuarios
                        .SelectMany(ru => ru.Rol.PermisosRolRecurso)
                        .GroupBy(p => p.Recurso.Codigo)
                        .Select(g => new PermisoUsuario(
                            g.Key,
                            g.Any(p => p.CanCreate),
                            g.Any(p => p.CanEdit),
                            g.Any(p => p.CanDelete),
                            g.Max(p => p.Alcance)))
                        .ToList();

                    usuarioContext = new UsuarioContext(
                        usuario.Id,
                        usuario.Email,
                        usuario.EmpleadoId,
                        usuario.Empleado.SeccionId,
                        permisos);

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
