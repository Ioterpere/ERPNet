using ERPNet.Application.Auth;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common.DTOs.Validators;
using ERPNet.Application.Common;
using ERPNet.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ERPNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IMaquinariaService, MaquinariaService>();
        services.AddScoped<IRolService, RolService>();
        services.AddValidatorsFromAssemblyContaining<CreateUsuarioRequestValidator>();
        return services;
    }
}
