using ERPNet.Web.Blazor;
using ERPNet.Web.Blazor.Bff;
using ERPNet.Web.Blazor.Components;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

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
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// Caché distribuido: almacena los tokens de ERPNet.Api por sesión.
// En producción sustituir por Redis u otro proveedor distribuido.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddAuthorization();

// Cliente HTTP apuntando a ERPNet.Api
builder.Services.AddHttpClient("ErpNetApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ErpNetApi:BaseUrl"]!);
});

builder.Services.AddHttpContextAccessor();

// BffApiClient inyecta automáticamente el Bearer token en cada llamada a ERPNet.Api
builder.Services.AddScoped<BffApiClient>();

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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ERPNet.Web.Blazor.Client.Components._Imports).Assembly);

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
