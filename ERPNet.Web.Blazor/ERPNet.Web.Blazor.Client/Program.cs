using ERPNet.ApiClient;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Clientes tipados — apuntan al BFF (mismo origen).
// La cookie de sesión se envía automáticamente; el BFF añade el Bearer al reenviar a la API.
builder.Services.AddApiClients(builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();
