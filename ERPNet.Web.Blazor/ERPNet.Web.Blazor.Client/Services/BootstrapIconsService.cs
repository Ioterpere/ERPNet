using System.Text.RegularExpressions;

namespace ERPNet.Web.Blazor.Client.Services;

public class BootstrapIconsService(HttpClient http)
{
    private static IReadOnlyList<string>? _cache;
    private static readonly Regex _regex = new(@"\.bi-([\w-]+)::before", RegexOptions.Compiled);

    public async Task<IReadOnlyList<string>> GetIconNamesAsync()
    {
        if (_cache is not null) return _cache;
        var css = await http.GetStringAsync("lib/bootstrap-icons/css/bootstrap-icons.min.css");
        _cache = _regex.Matches(css)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        return _cache;
    }
}
