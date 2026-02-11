using ERPNet.Application.Email;
using ERPNet.Infrastructure.Database;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Infrastructure.Email;
using ERPNet.Application.Interfaces;
using ERPNet.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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
        services.AddScoped<ILogService, LogService>();

        return services;
    }

    public static IServiceCollection AddEmailServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));

        services.AddMvcCore()
            .AddRazorViewEngine()
            .AddRazorRuntimeCompilation(options =>
            {
                options.FileProviders.Add(
                    new EmbeddedFileProvider(
                        typeof(EmailService).Assembly,
                        "ERPNet.Infrastructure.Email.Templates"));
            });

        services.AddScoped<RazorViewToStringRenderer>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
