using Microsoft.AspNetCore.Components;

namespace ERPNet.Web.Blazor.Client.Pages;

public partial class Login : ComponentBase
{
    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery]
    private bool Error { get; set; }

    private string? _error;

    protected override void OnParametersSet()
    {
        _error = Error ? "Credenciales incorrectas. Inténtalo de nuevo." : null;
    }
}
