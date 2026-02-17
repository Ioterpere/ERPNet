using System.Net.Http.Json;
using System.Web;
using ERPNet.Web.Blazor.Client.Auth;
using ERPNet.Web.Blazor.Shared.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ERPNet.Web.Blazor.Client.Pages;

public partial class Login(
    HttpClient http,
    NavigationManager nav,
    AuthenticationStateProvider authStateProvider)
    : ComponentBase
{
    private readonly BffLoginRequest _loginRequest = new();
    private string? _error;
    private bool _cargando;
    private string? _returnUrl;

    protected override void OnInitialized()
    {
        var uri = nav.ToAbsoluteUri(nav.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);
        _returnUrl = query["returnUrl"];
    }

    private async Task LoginAsync()
    {
        _error = null;
        _cargando = true;

        try
        {
            var response = await http.PostAsJsonAsync("/bff/login", _loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                _error = await GetErrorMessageAsync(response);
                return;
            }

            var usuario = await http.GetFromJsonAsync<UsuarioInfo>("/bff/me");
            if (usuario is null)
            {
                _error = "No se pudo obtener la información del usuario.";
                return;
            }

            ((BffAuthenticationStateProvider)authStateProvider).NotifyLogin(usuario);

            // Redirigir a la URL original o a home si no hay returnUrl
            var destination = !string.IsNullOrEmpty(_returnUrl) ? _returnUrl : "/";
            nav.NavigateTo(destination, forceLoad: true);
        }
        catch (HttpRequestException)
        {
            _error = "No se pudo conectar con el servidor.";
        }
        finally
        {
            _cargando = false;
        }
    }

    private static async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
            if (!string.IsNullOrEmpty(problem?.Detail))
                return problem.Detail;
        }
        catch { }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Credenciales incorrectas.",
            System.Net.HttpStatusCode.TooManyRequests => "Demasiados intentos. Intente de nuevo más tarde.",
            _ => "Error al iniciar sesión."
        };
    }

    private sealed class ProblemResponse
    {
        public string? Detail { get; set; }
    }
}
