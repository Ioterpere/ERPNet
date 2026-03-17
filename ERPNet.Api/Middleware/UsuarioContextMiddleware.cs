using System.Security.Claims;
using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Repositories;
using ERPNet.Domain.Enums;

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
            // Leer empresa activa del header X-Empresa-Id
            int? empresaId = null;
            if (context.Request.Headers.TryGetValue("X-Empresa-Id", out var empresaHeader)
                && int.TryParse(empresaHeader, out var parsedEmpresaId))
            {
                empresaId = parsedEmpresaId;
            }

            // Leer plataforma del header X-Plataforma
            Plataforma? plataforma = null;
            if (context.Request.Headers.TryGetValue("X-Plataforma", out var plataformaHeader)
                && Enum.TryParse<Plataforma>(plataformaHeader, out var parsedPlataforma))
            {
                plataforma = parsedPlataforma;
            }

            var cacheKey = $"usuario:{id}:{empresaId ?? 0}";
            var usuarioContext = cache.Get<UsuarioContext>(cacheKey);

            if (usuarioContext is null)
            {
                var usuario = await usuarioRepository.GetByIdConPermisosAsync(id);

                if (usuario is not null)
                {
                    var empresaIds = usuario.UsuarioEmpresas
                        .Select(ue => ue.EmpresaId)
                        .ToList();

                    // Validar que la empresa solicitada es accesible para el usuario
                    if (empresaId.HasValue && !empresaIds.Contains(empresaId.Value))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }

                    // Permisos = roles globales (EmpresaId == null) + roles de la empresa activa
                    var rolesAplicables = usuario.RolesUsuarios
                        .Where(ru => ru.EmpresaId == null || ru.EmpresaId == empresaId)
                        .ToList();

                    var permisos = rolesAplicables
                        .SelectMany(ru => ru.Rol.PermisosRolRecurso)
                        .GroupBy(p => (RecursoCodigo)p.RecursoId)
                        .Select(g => new PermisoResponse
                        {
                            Codigo = g.Key,
                            CanCreate = g.Any(p => p.CanCreate),
                            CanEdit = g.Any(p => p.CanEdit),
                            CanDelete = g.Any(p => p.CanDelete),
                            Alcance = g.Max(p => p.Alcance)
                        })
                        .ToList();

                    var rolIds = rolesAplicables
                        .Select(ru => ru.RolId)
                        .ToList();

                    var requiereCambio = usuario.CaducidadContrasena.HasValue
                        && usuario.CaducidadContrasena.Value < DateTime.UtcNow;

                    usuarioContext = new UsuarioContext
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        EmpleadoId = usuario.EmpleadoId,
                        SeccionId = usuario.Empleado?.SeccionId ?? 0,
                        EmpresaId = empresaId,
                        EmpresaIds = empresaIds,
                        Permisos = permisos,
                        RolIds = rolIds,
                        RequiereCambioContrasena = requiereCambio
                    };

                    cache.Set(cacheKey, usuarioContext);
                }
            }

            if (usuarioContext is not null)
            {
                // Enriquecer con datos de la petición actual (no forman parte del caché)
                context.Items["UsuarioContext"] = usuarioContext with { Plataforma = plataforma };
            }
        }

        await next(context);
    }
}
