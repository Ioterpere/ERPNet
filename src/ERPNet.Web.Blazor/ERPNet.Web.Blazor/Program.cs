using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Bff;
using ERPNet.Web.Blazor.Client.Components.Common.Toast;
using ERPNet.Web.Blazor.Client.Services;
using ERPNet.Web.Blazor.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

// Forwarded headers: necesario cuando hay un reverse proxy (Nginx) que termina TLS.
// Permite que UseHttpsRedirection y las cookies conozcan el esquema real (https).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // En entorno de pruebas aceptamos proxy desde cualquier red.
    // En producción restringir a la IP del proxy.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// Autenticación basada en cookie — el JWT de ERPNet.Api se guarda en
// el servidor (IDistributedCache), nunca viaja al navegador.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/authentication/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// Caché distribuido: almacena los tokens de ERPNet.Api por sesión.
// En producción sustituir por Redis u otro proveedor distribuido.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Cliente HTTP genérico usado por BffApiClient para inyectar el Bearer token manualmente
builder.Services.AddHttpClient("ErpNetApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ErpNetApi:BaseUrl"]!);
});

builder.Services.AddHttpContextAccessor();

// Servicios compartidos con el WASM client (deben registrarse en ambos contenedores DI)
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<PermisosService>();
builder.Services.AddScoped<MenuStateService>();
builder.Services.AddScoped<EmpresaStateService>();
builder.Services.AddScoped<PrefilladoService>();

// BffTokenService: gestiona tokens JWT en caché del servidor (get, refresh, invalidate)
builder.Services.AddScoped<BffTokenService>();

// BffAuthService: flujo login/logout usando IAuthClient (sin hardcoding de rutas)
builder.Services.AddScoped<BffAuthService>();

// BffAuthHandler: DelegatingHandler que inyecta el Bearer token en los clientes tipados
builder.Services.AddTransient<BffAuthHandler>();

// Clientes tipados generados por NSwag — con Bearer token inyectado via BffAuthHandler
builder.Services.AddApiClients(
    builder.Configuration["ErpNetApi:BaseUrl"]!,
    b => b.AddHttpMessageHandler<BffAuthHandler>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ERPNet.Web.Blazor.Client.Components._Imports).Assembly);

app.MapControllers();

// Proxy genérico BFF → API: captura cualquier /api/** del WASM (cookie auth),
// añade Bearer token y reenvía a ERPNet.Api. Sin boilerplate por endpoint.
app.MapBffProxy();

await app.RunAsync();
