using ERPNet.Api.Attributes;
using ERPNet.Api.Middleware;
using ERPNet.Application.Auth;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ERPNet.Testing.Tests;

public class ControlAccesoMiddlewareTests
{
    private bool _nextLlamado;

    private ControlAccesoMiddleware CrearMiddleware()
    {
        return new ControlAccesoMiddleware(_ =>
        {
            _nextLlamado = true;
            return Task.CompletedTask;
        });
    }

    private static Endpoint CrearEndpoint(RecursoAttribute? recurso = null, RequierePermisoAttribute? requierePermiso = null, SinPermisoAttribute? sinPermiso = null)
    {
        var metadata = new List<object>();
        if (recurso is not null) metadata.Add(recurso);
        if (requierePermiso is not null) metadata.Add(requierePermiso);
        if (sinPermiso is not null) metadata.Add(sinPermiso);

        return new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadata), "TestEndpoint");
    }

    private static DefaultHttpContext CrearHttpContext(string method, Endpoint? endpoint, UsuarioContext? usuarioCtx = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.SetEndpoint(endpoint);

        if (usuarioCtx is not null)
            context.Items["UsuarioContext"] = usuarioCtx;

        return context;
    }

    private static UsuarioContext CrearUsuarioContext(params PermisoUsuario[] permisos)
    {
        return new UsuarioContext(1, "test@erpnet.com", 1, 1, permisos.ToList(), []);
    }

    private static PermisoUsuario Permiso(RecursoCodigo codigo, bool create, bool edit, bool delete, Alcance alcance)
    {
        return new PermisoUsuario(codigo, create, edit, delete, alcance);
    }

    #region Sin control de acceso (pasa libre)

    [Fact(DisplayName = "Sin endpoint → pasa libre")]
    public async Task SinEndpoint_PasaLibre()
    {
        var context = CrearHttpContext("GET", endpoint: null);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    [Fact(DisplayName = "Sin [Recurso] → pasa libre")]
    public async Task SinRecurso_PasaLibre()
    {
        var endpoint = CrearEndpoint();
        var context = CrearHttpContext("GET", endpoint);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    [Fact(DisplayName = "[SinPermiso] → pasa libre aunque no tenga permisos")]
    public async Task SinPermisoAttr_PasaLibre()
    {
        var endpoint = CrearEndpoint(
            recurso: new RecursoAttribute(RecursoCodigo.Aplicacion),
            sinPermiso: new SinPermisoAttribute());
        var context = CrearHttpContext("GET", endpoint);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    #endregion

    #region Sin autenticación o sin permiso

    [Fact(DisplayName = "Sin UsuarioContext → 401")]
    public async Task SinUsuarioContext_401()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var context = CrearHttpContext("GET", endpoint);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(_nextLlamado);
    }

    [Fact(DisplayName = "Sin permiso sobre el recurso → 403")]
    public async Task SinPermisoRecurso_403()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Vacaciones, true, true, true, Alcance.Global));
        var context = CrearHttpContext("GET", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(_nextLlamado);
    }

    #endregion

    #region Convención por verbo HTTP

    [Fact(DisplayName = "GET con permiso sobre recurso → pasa")]
    public async Task GET_ConPermiso_Pasa()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, false, false, false, Alcance.Global));
        var context = CrearHttpContext("GET", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    [Fact(DisplayName = "POST sin CanCreate → 403")]
    public async Task POST_SinCanCreate_403()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, false, true, true, Alcance.Global));
        var context = CrearHttpContext("POST", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(_nextLlamado);
    }

    [Fact(DisplayName = "POST con CanCreate → pasa")]
    public async Task POST_ConCanCreate_Pasa()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, true, false, false, Alcance.Global));
        var context = CrearHttpContext("POST", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    [Fact(DisplayName = "PUT sin CanEdit → 403")]
    public async Task PUT_SinCanEdit_403()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, true, false, true, Alcance.Global));
        var context = CrearHttpContext("PUT", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(_nextLlamado);
    }

    [Fact(DisplayName = "PUT con CanEdit → pasa")]
    public async Task PUT_ConCanEdit_Pasa()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, false, true, false, Alcance.Global));
        var context = CrearHttpContext("PUT", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    [Fact(DisplayName = "DELETE sin CanDelete → 403")]
    public async Task DELETE_SinCanDelete_403()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, true, true, false, Alcance.Global));
        var context = CrearHttpContext("DELETE", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(_nextLlamado);
    }

    [Fact(DisplayName = "DELETE con CanDelete → pasa")]
    public async Task DELETE_ConCanDelete_Pasa()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, false, false, true, Alcance.Global));
        var context = CrearHttpContext("DELETE", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    [Fact(DisplayName = "PATCH sin CanEdit → 403")]
    public async Task PATCH_SinCanEdit_403()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, true, false, true, Alcance.Global));
        var context = CrearHttpContext("PATCH", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(_nextLlamado);
    }

    #endregion

    #region Override con [RequierePermiso]

    [Fact(DisplayName = "[RequierePermiso(CanDelete)] en POST → verifica CanDelete, no CanCreate")]
    public async Task RequierePermisoOverride_UsaFlags()
    {
        var endpoint = CrearEndpoint(
            recurso: new RecursoAttribute(RecursoCodigo.Aplicacion),
            requierePermiso: new RequierePermisoAttribute { CanDelete = true });
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, true, true, false, Alcance.Global));
        var context = CrearHttpContext("POST", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(_nextLlamado);
    }

    [Fact(DisplayName = "[RequierePermiso(CanCreate)] en GET → verifica CanCreate")]
    public async Task RequierePermisoOverrideCreate_EnGET()
    {
        var endpoint = CrearEndpoint(
            recurso: new RecursoAttribute(RecursoCodigo.Aplicacion),
            requierePermiso: new RequierePermisoAttribute { CanCreate = true });
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, true, false, false, Alcance.Global));
        var context = CrearHttpContext("GET", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.True(_nextLlamado);
    }

    #endregion

    #region Alcance

    [Fact(DisplayName = "Permiso válido → deja Alcance en HttpContext.Items")]
    public async Task PermisoValido_DejaAlcance()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext(
            Permiso(RecursoCodigo.Aplicacion, true, true, true, Alcance.Seccion));
        var context = CrearHttpContext("GET", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.Equal(Alcance.Seccion, context.Items["Alcance"]);
    }

    [Fact(DisplayName = "403 → no deja Alcance en HttpContext.Items")]
    public async Task Forbidden_NoDejaAlcance()
    {
        var endpoint = CrearEndpoint(recurso: new RecursoAttribute(RecursoCodigo.Aplicacion));
        var usuarioCtx = CrearUsuarioContext();
        var context = CrearHttpContext("GET", endpoint, usuarioCtx);
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(context);

        Assert.False(context.Items.ContainsKey("Alcance"));
    }

    #endregion
}
