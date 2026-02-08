using ERPNet.Application.Auth;
using ERPNet.Application.Auth.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ERPNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService,AuthService>();
        return services;
    }
}
