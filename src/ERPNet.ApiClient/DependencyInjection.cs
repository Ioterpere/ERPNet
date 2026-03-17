using Microsoft.Extensions.DependencyInjection;

namespace ERPNet.ApiClient;

public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los clientes tipados de ERPNet.Api generados por NSwag.
    /// Escanea el assembly buscando clases *Client que implementen I*Client,
    /// y las registra como typed HTTP clients con la URL base configurada.
    /// </summary>
    public static IServiceCollection AddApiClients(
        this IServiceCollection services,
        string baseUrl,
        Action<IHttpClientBuilder>? configure = null)
    {
        var clientTypes = typeof(EmpleadosClient).Assembly
            .GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Client"));

        foreach (var clientType in clientTypes)
        {
            var interfaceType = clientType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{clientType.Name}");

            if (interfaceType is null) continue;

            // Captura local para el closure
            var type = clientType;
            var name = clientType.Name;

            var builder = services.AddHttpClient(name, c => c.BaseAddress = new Uri(baseUrl));
            configure?.Invoke(builder);

            services.AddTransient(interfaceType, sp =>
            {
                var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(name);
                return Activator.CreateInstance(type, http)!;
            });
        }

        return services;
    }
}
