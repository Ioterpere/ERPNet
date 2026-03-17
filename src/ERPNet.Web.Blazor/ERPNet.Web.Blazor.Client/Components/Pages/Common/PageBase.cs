using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Common;

/// <summary>Clase base mínima para todas las páginas del ERP. Expone NombreEmpresa.</summary>
public abstract class PageBase : ComponentBase
{
    [CascadingParameter] protected Task<AuthenticationState>? AuthStateTask { get; set; }

    protected string NombreEmpresa =>
        AuthStateTask?.IsCompletedSuccessfully == true
            ? AuthStateTask.Result.User.FindFirst("empresa_nombre")?.Value ?? "ERPNet"
            : "ERPNet";
}
