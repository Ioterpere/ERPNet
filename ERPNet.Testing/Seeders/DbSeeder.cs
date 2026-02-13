using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace ERPNet.Testing.Seeders;

public class DbSeeder(ITestOutputHelper output)
{
    private static readonly string ConnectionString = new ConfigurationBuilder()
        .SetBasePath(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ERPNet.Api")))
        .AddJsonFile("appsettings.json")
        .Build()
        .GetConnectionString("DefaultConnection")!;

    [Fact(DisplayName = "Seed: Migrar BD y crear datos iniciales")]
    public async Task SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<ERPNetDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        using var context = new ERPNetDbContext(options);

        output.WriteLine("Eliminando BD existente...");
        await context.Database.EnsureDeletedAsync();
        output.WriteLine("BD eliminada.");

        output.WriteLine("Aplicando migraciones...");
        await context.Database.MigrateAsync();
        output.WriteLine("Migraciones aplicadas.");

        output.WriteLine("Insertando datos de prueba...");

        // Secciones
        var seccionAdmin = new Seccion { Nombre = "Administracion", CreatedAt = DateTime.UtcNow };
        var seccionProduccion = new Seccion { Nombre = "Produccion", CreatedAt = DateTime.UtcNow };
        context.Secciones.AddRange(seccionAdmin, seccionProduccion);
        await context.SaveChangesAsync();

        // Empleados
        var empAdmin = new Empleado
        {
            Nombre = "Admin", Apellidos = "Sistema", DNI = Dni.From("00000000T"),
            Activo = true, SeccionId = seccionAdmin.Id, CreatedAt = DateTime.UtcNow,
        };
        var empEncargado = new Empleado
        {
            Nombre = "Carlos", Apellidos = "Lopez", DNI = Dni.From("11111111H"),
            Activo = true, SeccionId = seccionProduccion.Id, CreatedAt = DateTime.UtcNow,
        };
        var empOperario = new Empleado
        {
            Nombre = "Maria", Apellidos = "Garcia", DNI = Dni.From("22222222J"),
            Activo = true, SeccionId = seccionProduccion.Id, CreatedAt = DateTime.UtcNow,
        };
        context.Empleados.AddRange(empAdmin, empEncargado, empOperario);
        await context.SaveChangesAsync();

        // Menus (vinculados a recursos — los Recursos se crean via HasData en la migracion)
        var menuVacaciones = new Menu
        {
            Nombre = "Vacaciones", Path = "/vacaciones", Orden = 1,
            Plataforma = Plataforma.Web, RecursoId = (int)RecursoCodigo.Vacaciones,
            CreatedAt = DateTime.UtcNow,
        };
        var menuEmpleados = new Menu
        {
            Nombre = "Empleados", Path = "/empleados", Orden = 2,
            Plataforma = Plataforma.Web, RecursoId = (int)RecursoCodigo.Empleados,
            CreatedAt = DateTime.UtcNow,
        };
        var menuTurnos = new Menu
        {
            Nombre = "Turnos", Path = "/turnos", Orden = 3,
            Plataforma = Plataforma.Web, RecursoId = (int)RecursoCodigo.Turnos,
            CreatedAt = DateTime.UtcNow,
        };
        context.Menus.AddRange(menuVacaciones, menuEmpleados, menuTurnos);
        await context.SaveChangesAsync();

        // Roles
        var rolAdmin = new Rol
        {
            Nombre = "Administrador", Descripcion = "Acceso total",
            CreatedAt = DateTime.UtcNow,
        };
        var rolEncargado = new Rol
        {
            Nombre = "Encargado", Descripcion = "Gestion de su seccion",
            CreatedAt = DateTime.UtcNow,
        };
        var rolEmpleado = new Rol
        {
            Nombre = "Empleado", Descripcion = "Acceso basico",
            CreatedAt = DateTime.UtcNow,
        };
        context.Roles.AddRange(rolAdmin, rolEncargado, rolEmpleado);
        await context.SaveChangesAsync();

        // Permisos
        // Admin: acceso Global con todos los flags a TODOS los recursos
        var permisosAdmin = Enum.GetValues<RecursoCodigo>()
            .Select(r => new PermisoRolRecurso
            {
                RolId = rolAdmin.Id, RecursoId = (int)r,
                CanCreate = true, CanEdit = true, CanDelete = true, Alcance = Alcance.Global,
            });

        context.PermisosRolRecurso.AddRange(permisosAdmin);
        context.PermisosRolRecurso.AddRange(
            // Encargado: Vacaciones con Create a nivel Seccion
            new PermisoRolRecurso
            {
                RolId = rolEncargado.Id, RecursoId = (int)RecursoCodigo.Vacaciones,
                CanCreate = true, CanEdit = false, CanDelete = false, Alcance = Alcance.Seccion,
            },
            // Encargado: Turnos puede editar a nivel de Seccion
            new PermisoRolRecurso
            {
                RolId = rolEncargado.Id, RecursoId = (int)RecursoCodigo.Turnos,
                CanCreate = false, CanEdit = false, CanDelete = false, Alcance = Alcance.Seccion,
            },
            // Empleado: Vacaciones solo lectura sobre sus propios datos
            new PermisoRolRecurso
            {
                RolId = rolEmpleado.Id, RecursoId = (int)RecursoCodigo.Vacaciones,
                CanCreate = false, CanEdit = false, CanDelete = false, Alcance = Alcance.Propio,
            },
            // Empleado: Turnos solo lectura sobre sus propios datos
            new PermisoRolRecurso
            {
                RolId = rolEmpleado.Id, RecursoId = (int)RecursoCodigo.Turnos,
                CanCreate = false, CanEdit = false, CanDelete = false, Alcance = Alcance.Propio,
            });

        await context.SaveChangesAsync();

        // Usuarios
        var usuarioAdmin = new Usuario
        {
            Email = Email.From("admin@erpnet.com"),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Activo = true, EmpleadoId = empAdmin.Id, CreatedAt = DateTime.UtcNow,
        };
        var usuarioEncargado = new Usuario
        {
            Email = Email.From("carlos@erpnet.com"),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Carlos123!"),
            Activo = true, EmpleadoId = empEncargado.Id, CreatedAt = DateTime.UtcNow,
        };
        var usuarioOperario = new Usuario
        {
            Email = Email.From("maria@erpnet.com"),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Maria123!"),
            Activo = true, EmpleadoId = empOperario.Id, CreatedAt = DateTime.UtcNow,
        };
        context.Usuarios.AddRange(usuarioAdmin, usuarioEncargado, usuarioOperario);
        await context.SaveChangesAsync();

        // Asignar roles
        context.RolesUsuarios.AddRange(
            new RolUsuario { UsuarioId = usuarioAdmin.Id, RolId = rolAdmin.Id },
            // Encargado tiene rol Encargado + Empleado (para probar merge)
            new RolUsuario { UsuarioId = usuarioEncargado.Id, RolId = rolEncargado.Id },
            new RolUsuario { UsuarioId = usuarioEncargado.Id, RolId = rolEmpleado.Id },
            new RolUsuario { UsuarioId = usuarioOperario.Id, RolId = rolEmpleado.Id });
        await context.SaveChangesAsync();

        output.WriteLine("Seed completado.");
        output.WriteLine("  admin@erpnet.com / Admin123! (Administrador)");
        output.WriteLine("  carlos@erpnet.com / Carlos123! (Encargado + Empleado)");
        output.WriteLine("  maria@erpnet.com / Maria123! (Empleado)");
    }

}
