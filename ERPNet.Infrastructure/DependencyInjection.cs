using ERPNet.Infrastructure.Database;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERPNet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ERPNetDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Scan(scan => scan
            .FromAssemblyOf<ERPNetDbContext>()
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
