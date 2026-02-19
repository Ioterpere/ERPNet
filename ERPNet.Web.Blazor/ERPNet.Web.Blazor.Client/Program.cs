using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Cliente interno para llamadas BFF (refresh, etc.) — sin BffAuthRetryHandler para evitar recursión.
builder.Services.AddHttpClient("BffInternal",
    c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddTransient<BffAuthRetryHandler>();

// Clientes tipados — apuntan al BFF (mismo origen).
// La cookie de sesión se envía automáticamente; el BFF añade el Bearer al reenviar a la API.
// BffAuthRetryHandler intercepta 401/403 antes de que lleguen a los componentes.
builder.Services.AddApiClients(
    builder.HostEnvironment.BaseAddress,
    b => b.AddHttpMessageHandler<BffAuthRetryHandler>());

builder.Services.AddScoped<ToastService>();

await builder.Build().RunAsync();
