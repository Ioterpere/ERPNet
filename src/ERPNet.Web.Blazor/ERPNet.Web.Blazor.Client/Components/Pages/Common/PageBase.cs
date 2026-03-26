using ERPNet.Web.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ERPNet.Web.Blazor.Client.Components.Pages.Common;

/// <summary>Clase base mínima para todas las páginas del ERP. Expone NombreEmpresa.</summary>
public abstract class PageBase : ComponentBase
{
    [CascadingParameter] protected Task<AuthenticationState>? AuthStateTask { get; set; }
    [Inject] protected EmpresaStateService EmpresaState { get; set; } = default!;

    /// <summary>
    /// Nombre de la empresa activa. MainLayout inicializa <see cref="EmpresaStateService"/>
    /// siempre antes de que cualquier página renderice, por lo que el valor es fiable.
    /// </summary>
    protected string NombreEmpresa => EmpresaState.EmpresaNombre ?? "ERPNet";
}
