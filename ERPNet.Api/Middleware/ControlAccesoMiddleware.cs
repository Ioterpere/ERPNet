using ERPNet.Api.Attributes;
using ERPNet.Application.Auth;

namespace ERPNet.Api.Middleware;

public class ControlAccesoMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is null)
        {
            await next(context);
            return;
        }

        // Sin [Recurso] en el controller → no se aplica control de acceso
        var recursoAttr = endpoint.Metadata.GetMetadata<RecursoAttribute>();

        if (recursoAttr is null)
        {
            await next(context);
            return;
        }

        // Obtener contexto de usuario (movido antes de [SinPermiso])
        var usuarioContext = context.Items["UsuarioContext"] as UsuarioContext;

        if (usuarioContext is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var esSinPermiso = endpoint.Metadata.GetMetadata<SinPermisoAttribute>() is not null;

        // Contraseña caducada: solo permitir endpoints marcados con [PermitirContrasenaCaducada]
        if (usuarioContext.RequiereCambioContrasena)
        {
            if (endpoint.Metadata.GetMetadata<PermitirContrasenaCaducadaAttribute>() is not null)
            {
                await next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { detail = "Cambio de contraseña requerido." });
            return;
        }

        // [SinPermiso] en el action → se salta el control
        if (esSinPermiso)
        {
            await next(context);
            return;
        }

        var permiso = usuarioContext.Permisos.FirstOrDefault(p => p.Codigo == recursoAttr.Codigo);

        if (permiso is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        // Determinar qué flags verificar: override explícito o convención por verbo HTTP
        var override_ = endpoint.Metadata.GetMetadata<RequierePermisoAttribute>();
        bool requiereCreate;
        bool requiereEdit;
        bool requiereDelete;

        if (override_ is not null)
        {
            requiereCreate = override_.CanCreate;
            requiereEdit = override_.CanEdit;
            requiereDelete = override_.CanDelete;
        }
        else
        {
            var method = context.Request.Method;
            requiereCreate = method == HttpMethods.Post;
            requiereEdit = method == HttpMethods.Put || method == HttpMethods.Patch;
            requiereDelete = method == HttpMethods.Delete;
        }

        if (requiereCreate && !permiso.CanCreate ||
            requiereEdit && !permiso.CanEdit ||
            requiereDelete && !permiso.CanDelete)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        // Dejar el Alcance resuelto para que los repositorios lo consuman
        context.Items["Alcance"] = permiso.Alcance;

        await next(context);
    }
}
