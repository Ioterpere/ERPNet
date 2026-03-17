namespace ERPNet.Application;

public class ErpNetSettings
{
    public string ErpWebClient { get; set; } = string.Empty;
    public List<string> AllowedOrigins { get; set; } = [];
}
