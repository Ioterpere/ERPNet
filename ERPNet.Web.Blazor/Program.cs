using ERPNet.Web.Blazor;
using ERPNet.Web.Blazor.Auth;
using ERPNet.Web.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// BFF settings
builder.Services.Configure<BffSettings>(config.GetSection("BffSettings"));
var apiBaseUrl = config["BffSettings:ApiBaseUrl"]!;

// Data Protection (cookie encryption)
builder.Services.AddDataProtection();

// HttpClient for BFF → API calls
builder.Services.AddHttpClient("erpnet-api", c => c.BaseAddress = new Uri(apiBaseUrl));

// Token cookie service
builder.Services.AddScoped<ITokenCookieService, TokenCookieService>();

// YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(config.GetSection("ReverseProxy"))
    .AddTransforms<BffTokenTransformProvider>();

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
app.UseAntiforgery();

// BFF auth endpoints (/bff/login, /bff/logout, /bff/me)
app.MapBffAuthEndpoints();

// YARP proxy (/api/* → ERPNet.Api)
app.MapReverseProxy();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ERPNet.Web.Blazor.Client._Imports).Assembly);

app.Run();
