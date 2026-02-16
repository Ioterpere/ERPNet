namespace ERPNet.Web.Blazor;

public class BffSettings
{
    public string ApiBaseUrl { get; set; } = null!;
    public string CookieName { get; set; } = "__Host-erpnet";
    public int TokenRefreshBufferMinutes { get; set; } = 2;
}
