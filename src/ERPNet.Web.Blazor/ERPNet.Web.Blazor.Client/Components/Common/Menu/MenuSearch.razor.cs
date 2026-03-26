using ERPNet.ApiClient;
using ERPNet.Web.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;

namespace ERPNet.Web.Blazor.Client.Components.Common.Menu;

public partial class MenuSearch
{
    [Inject] private IMenusClient MenusClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private MenuStateService MenuState { get; set; } = default!;

    private sealed record MenuItem(string Nombre, string Path, string? IconClass, string? Padre);

    private List<MenuItem> _todosItems = [];

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive) return;
        try
        {
            var menus = await MenuState.ObtenerAsync(MenusClient);
            _todosItems.Add(new("Inicio", "/", "bi-house-door-fill", null));
            foreach (var menu in menus)
            {
                if (menu.SubMenus?.Count > 0)
                {
                    foreach (var sub in menu.SubMenus)
                    {
                        if (sub.SubMenus?.Count > 0)
                        {
                            foreach (var sub3 in sub.SubMenus.Where(s => !string.IsNullOrEmpty(s.Path)))
                                _todosItems.Add(new(sub3.Nombre, sub3.Path!, sub3.IconClass, $"{menu.Nombre} / {sub.Nombre}"));
                        }
                        else if (!string.IsNullOrEmpty(sub.Path))
                        {
                            _todosItems.Add(new(sub.Nombre, sub.Path!, sub.IconClass, menu.Nombre));
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(menu.Path))
                {
                    _todosItems.Add(new(menu.Nombre, menu.Path, menu.IconClass, null));
                }
            }
        }
        catch { }
    }

    private Task<IEnumerable<MenuItem>> BuscarAsync(string query, CancellationToken _)
    {
        IEnumerable<MenuItem> resultados = _todosItems
            .Where(m => m.Nombre.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || (m.Padre?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(8);
        return Task.FromResult(resultados);
    }

    private void OnItemNavigate(MenuItem item) =>
        Nav.NavigateTo(item.Path == "/" ? "" : item.Path.TrimStart('/'));
}
