using Microsoft.AspNetCore.Components;

namespace ERPNet.Web.Blazor.Client;

/// <summary>
/// Excepción lanzada por <see cref="BffAuthRetryHandler"/> cuando un 401 o 403
/// ya ha sido gestionado (navegación a login o toast). Los componentes la capturan
/// para suprimir el mensaje de error genérico.
/// </summary>
public sealed class BffAuthException(int statusCode) : Exception
{
    public int StatusCode { get; } = statusCode;
}

/// <summary>
/// DelegatingHandler que intercepta 401 y 403 en el WASM antes de que lleguen
/// a los componentes:
///
/// - 401: llama a POST /bff/refresh. Si el BFF refresca el token (OK), recarga
///        la página. Si no puede (sesión muerta), redirige a login.
/// - 403: muestra un toast de aviso y lanza <see cref="BffAuthException"/>.
///
/// El componente solo necesita capturar <see cref="BffAuthException"/> y
/// ignorarla — el handler ya tomó la acción correspondiente.
/// </summary>
public sealed class BffAuthRetryHandler(
    NavigationManager nav,
    ToastService toast,
    IHttpClientFactory http) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var response = await base.SendAsync(request, ct);

        if ((int)response.StatusCode == 401)
        {
            var refrescado = await TryRefreshAsync(ct);
            if (refrescado)
                nav.NavigateTo(nav.Uri, forceLoad: true);
            else
                nav.NavigateTo(
                    $"/authentication/login?returnUrl={Uri.EscapeDataString(nav.Uri)}",
                    forceLoad: true);

            throw new BffAuthException(401);
        }

        if ((int)response.StatusCode == 403)
        {
            toast.Aviso("Sin permiso para acceder a este recurso.");
            throw new BffAuthException(403);
        }

        return response;
    }

    /// <summary>
    /// Pregunta al BFF si puede refrescar el access token actual.
    /// Usa un cliente sin el handler para evitar recursión.
    /// </summary>
    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        try
        {
            var client = http.CreateClient("BffInternal");
            var resp = await client.PostAsync("/bff/refresh", null, ct);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
