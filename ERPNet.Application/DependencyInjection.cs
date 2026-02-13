using ERPNet.Application.Auth;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.DTOs.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ERPNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddValidatorsFromAssemblyContaining<CreateUsuarioRequestValidator>();
        return services;
    }
}
