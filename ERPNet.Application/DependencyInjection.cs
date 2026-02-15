using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ERPNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<UsuarioService>()
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddValidatorsFromAssemblyContaining<CreateUsuarioRequestValidator>();
        return services;
    }
}
