using System.Security.Claims;
using ERPNet.Api.Middleware;
using ERPNet.Application.Auth;
using ERPNet.Application.Interfaces;
using ERPNet.Domain.Repositories;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.Tests;

public class UsuarioContextMiddlewareTests
{
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();

    private UsuarioContextMiddleware CrearMiddleware(RequestDelegate? next = null)
    {
        next ??= _ => Task.CompletedTask;
        return new UsuarioContextMiddleware(next);
    }

    private static DefaultHttpContext CrearHttpContext(int userId)
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return context;
    }

    private static Usuario CrearUsuario(int id, int seccionId, params RolUsuario[] roles)
    {
        var empleado = new Empleado
        {
            Id = 1, Nombre = "Test", Apellidos = "User", DNI = "00000000A",
            Activo = true, SeccionId = seccionId,
        };

        return new Usuario
        {
            Id = id, Email = "test@erpnet.com", PasswordHash = "hash",
            Activo = true, EmpleadoId = 1, Empleado = empleado,
            RolesUsuarios = roles.ToList(),
        };
    }

    private static RolUsuario CrearRolConPermisos(int rolId, params PermisoRolRecurso[] permisos)
    {
        var rol = new Rol { Id = rolId, Nombre = $"Rol{rolId}" };
        foreach (var p in permisos)
            rol.PermisosRolRecurso.Add(p);

        return new RolUsuario { RolId = rolId, Rol = rol };
    }

    private static PermisoRolRecurso Permiso(RecursoCodigo recurso, bool create, bool edit, bool delete, Alcance alcance) =>
        new() { RecursoId = (int)recurso, CanCreate = create, CanEdit = edit, CanDelete = delete, Alcance = alcance };

    private UsuarioContext? EjecutarMiddleware(Usuario usuario)
    {
        var httpContext = CrearHttpContext(usuario.Id);
        _cache.Get<UsuarioContext>(Arg.Any<string>()).Returns((UsuarioContext?)null);
        _usuarioRepo.GetByIdConPermisosAsync(usuario.Id).Returns(usuario);

        var middleware = CrearMiddleware();
        middleware.InvokeAsync(httpContext, _cache, _usuarioRepo).GetAwaiter().GetResult();

        return httpContext.Items["UsuarioContext"] as UsuarioContext;
    }

    #region Datos basicos del contexto

    [Fact(DisplayName = "Middleware: sin claim de usuario no setea contexto")]
    public async Task SinClaim_NoSeteaContexto()
    {
        var httpContext = new DefaultHttpContext();
        var middleware = CrearMiddleware();

        await middleware.InvokeAsync(httpContext, _cache, _usuarioRepo);

        Assert.Null(httpContext.Items["UsuarioContext"]);
    }

    [Fact(DisplayName = "Middleware: usuario valido setea Id, Email, EmpleadoId y SeccionId")]
    public void UsuarioValido_SeteaDatosBasicos()
    {
        var usuario = CrearUsuario(5, seccionId: 3,
            CrearRolConPermisos(1, Permiso(RecursoCodigo.Vacaciones, true, false, false, Alcance.Propio)));

        var ctx = EjecutarMiddleware(usuario);

        Assert.NotNull(ctx);
        Assert.Equal(5, ctx.Id);
        Assert.Equal("test@erpnet.com", ctx.Email);
        Assert.Equal(1, ctx.EmpleadoId);
        Assert.Equal(3, ctx.SeccionId);
    }

    [Fact(DisplayName = "Middleware: usuario inexistente no setea contexto")]
    public async Task UsuarioInexistente_NoSeteaContexto()
    {
        var httpContext = CrearHttpContext(999);
        _cache.Get<UsuarioContext>(Arg.Any<string>()).Returns((UsuarioContext?)null);
        _usuarioRepo.GetByIdConPermisosAsync(999).Returns((Usuario?)null);

        var middleware = CrearMiddleware();
        await middleware.InvokeAsync(httpContext, _cache, _usuarioRepo);

        Assert.Null(httpContext.Items["UsuarioContext"]);
    }

    [Fact(DisplayName = "Middleware: cache hit no consulta repositorio")]
    public async Task CacheHit_NoConsultaRepo()
    {
        var httpContext = CrearHttpContext(1);
        var cached = new UsuarioContext(1, "cached@test.com", 1, 1, []);
        _cache.Get<UsuarioContext>("usuario:1").Returns(cached);

        var middleware = CrearMiddleware();
        await middleware.InvokeAsync(httpContext, _cache, _usuarioRepo);

        var ctx = httpContext.Items["UsuarioContext"] as UsuarioContext;
        Assert.NotNull(ctx);
        Assert.Equal("cached@test.com", ctx.Email);
        await _usuarioRepo.DidNotReceive().GetByIdConPermisosAsync(Arg.Any<int>());
    }

    #endregion

    #region Merge de permisos

    [Fact(DisplayName = "Middleware: un rol con un recurso se mapea directamente")]
    public void UnRol_UnRecurso_MapeaDirecto()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1, Permiso(RecursoCodigo.Vacaciones, true, false, false, Alcance.Propio)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Single(ctx.Permisos);
        var p = ctx.Permisos[0];
        Assert.Equal(RecursoCodigo.Vacaciones, p.Codigo);
        Assert.True(p.CanCreate);
        Assert.False(p.CanEdit);
        Assert.False(p.CanDelete);
        Assert.Equal(Alcance.Propio, p.Alcance);
    }

    [Fact(DisplayName = "Middleware: rol sin permisos genera lista vacia")]
    public void RolSinPermisos_ListaVacia()
    {
        var usuario = CrearUsuario(1, 1, CrearRolConPermisos(1));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Empty(ctx.Permisos);
    }

    [Fact(DisplayName = "Middleware: un rol con multiples recursos genera una entrada por recurso")]
    public void UnRol_MultiplesRecursos_EntradaPorRecurso()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1,
                Permiso(RecursoCodigo.Vacaciones, true, true, true, Alcance.Global),
                Permiso(RecursoCodigo.Turnos, true, false, false, Alcance.Propio)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Equal(2, ctx.Permisos.Count);
        Assert.Contains(ctx.Permisos, p => p.Codigo == RecursoCodigo.Vacaciones);
        Assert.Contains(ctx.Permisos, p => p.Codigo == RecursoCodigo.Turnos);
    }

    [Fact(DisplayName = "Middleware: dos roles mismo recurso - booleanos se combinan con OR")]
    public void DosRoles_MismoRecurso_BooleanosOR()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1, Permiso(RecursoCodigo.Vacaciones, true, false, false, Alcance.Propio)),
            CrearRolConPermisos(2, Permiso(RecursoCodigo.Vacaciones, false, true, false, Alcance.Propio)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Single(ctx.Permisos);
        var p = ctx.Permisos[0];
        Assert.True(p.CanCreate);
        Assert.True(p.CanEdit);
        Assert.False(p.CanDelete);
    }

    [Fact(DisplayName = "Middleware: dos roles mismo recurso - alcance toma el mas amplio")]
    public void DosRoles_MismoRecurso_AlcanceMaximo()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1, Permiso(RecursoCodigo.Vacaciones, true, false, false, Alcance.Propio)),
            CrearRolConPermisos(2, Permiso(RecursoCodigo.Vacaciones, false, true, false, Alcance.Seccion)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Single(ctx.Permisos);
        Assert.Equal(Alcance.Seccion, ctx.Permisos[0].Alcance);
    }

    [Fact(DisplayName = "Middleware: tres roles mismo recurso - Global gana sobre Seccion y Propio")]
    public void TresRoles_MismoRecurso_GlobalGana()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1, Permiso(RecursoCodigo.Vacaciones, false, false, false, Alcance.Propio)),
            CrearRolConPermisos(2, Permiso(RecursoCodigo.Vacaciones, false, false, false, Alcance.Seccion)),
            CrearRolConPermisos(3, Permiso(RecursoCodigo.Vacaciones, false, false, false, Alcance.Global)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Single(ctx.Permisos);
        Assert.Equal(Alcance.Global, ctx.Permisos[0].Alcance);
    }

    [Fact(DisplayName = "Middleware: dos roles recursos distintos no se mezclan")]
    public void DosRoles_RecursosDistintos_NoSeMezclan()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1, Permiso(RecursoCodigo.Vacaciones, true, false, false, Alcance.Seccion)),
            CrearRolConPermisos(2, Permiso(RecursoCodigo.Turnos, false, true, false, Alcance.Global)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Equal(2, ctx.Permisos.Count);

        var pVacaciones = ctx.Permisos.First(p => p.Codigo == RecursoCodigo.Vacaciones);
        Assert.True(pVacaciones.CanCreate);
        Assert.False(pVacaciones.CanEdit);
        Assert.Equal(Alcance.Seccion, pVacaciones.Alcance);

        var pTurnos = ctx.Permisos.First(p => p.Codigo == RecursoCodigo.Turnos);
        Assert.False(pTurnos.CanCreate);
        Assert.True(pTurnos.CanEdit);
        Assert.Equal(Alcance.Global, pTurnos.Alcance);
    }

    #endregion

    #region Caso real: Encargado + Empleado

    [Fact(DisplayName = "Middleware: Encargado+Empleado en Vacaciones merge Create+Edit Seccion")]
    public void EncargadoMasEmpleado_Vacaciones_MergeCorrecto()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1, Permiso(RecursoCodigo.Vacaciones, true, true, false, Alcance.Seccion)),
            CrearRolConPermisos(2, Permiso(RecursoCodigo.Vacaciones, true, false, false, Alcance.Propio)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Single(ctx.Permisos);
        var p = ctx.Permisos[0];
        Assert.True(p.CanCreate);
        Assert.True(p.CanEdit);
        Assert.False(p.CanDelete);
        Assert.Equal(Alcance.Seccion, p.Alcance);
    }

    [Fact(DisplayName = "Middleware: Encargado+Empleado multiples recursos merge completo")]
    public void EncargadoMasEmpleado_MultiplesRecursos_MergeCompleto()
    {
        var usuario = CrearUsuario(1, 1,
            CrearRolConPermisos(1,
                Permiso(RecursoCodigo.Vacaciones, true, true, false, Alcance.Seccion),
                Permiso(RecursoCodigo.Turnos, false, false, false, Alcance.Seccion)),
            CrearRolConPermisos(2,
                Permiso(RecursoCodigo.Vacaciones, true, false, false, Alcance.Propio)));

        var ctx = EjecutarMiddleware(usuario)!;

        Assert.Equal(2, ctx.Permisos.Count);

        var pVacaciones = ctx.Permisos.First(p => p.Codigo == RecursoCodigo.Vacaciones);
        Assert.True(pVacaciones.CanCreate);
        Assert.True(pVacaciones.CanEdit);
        Assert.False(pVacaciones.CanDelete);
        Assert.Equal(Alcance.Seccion, pVacaciones.Alcance);

        var pTurnos = ctx.Permisos.First(p => p.Codigo == RecursoCodigo.Turnos);
        Assert.False(pTurnos.CanCreate);
        Assert.False(pTurnos.CanEdit);
        Assert.False(pTurnos.CanDelete);
        Assert.Equal(Alcance.Seccion, pTurnos.Alcance);
    }

    #endregion

    #region Siempre llama a next

    [Fact(DisplayName = "Middleware: siempre ejecuta next aunque no haya usuario")]
    public async Task SiempreEjecutaNext()
    {
        var nextLlamado = false;
        var middleware = CrearMiddleware(_ => { nextLlamado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(new DefaultHttpContext(), _cache, _usuarioRepo);

        Assert.True(nextLlamado);
    }

    #endregion
}
