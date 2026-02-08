using ERPNet.Domain.Common;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Database.Context;

public class ERPNetDbContext(DbContextOptions<ERPNetDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<RolUsuario> RolesUsuarios => Set<RolUsuario>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<PermisoRolMenu> PermisosRolMenu => Set<PermisoRolMenu>();
    public DbSet<Log> Logs => Set<Log>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LogIntentoLogin> IntentosLogin => Set<LogIntentoLogin>();

    public DbSet<Seccion> Secciones => Set<Seccion>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<AsignacionTurno> AsignacionesTurno => Set<AsignacionTurno>();
    public DbSet<Vacacion> Vacaciones => Set<Vacacion>();
    public DbSet<Marcaje> Marcajes => Set<Marcaje>();
    public DbSet<IncidenciaMarcaje> IncidenciasMarcaje => Set<IncidenciaMarcaje>();

    public DbSet<Maquinaria> Maquinarias => Set<Maquinaria>();
    public DbSet<TipoMantenimiento> TiposMantenimiento => Set<TipoMantenimiento>();
    public DbSet<OrdenMantenimiento> OrdenesMantenimiento => Set<OrdenMantenimiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ERPNetDbContext).Assembly);

        // Global query filter para soft delete en todas las entidades que hereden de BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ERPNetDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, [modelBuilder]);
            }
        }
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }
}
