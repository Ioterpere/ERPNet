namespace ERPNet.Application.Ai;

public class AiSettings
{
    public bool Habilitado { get; set; } = false;
    public string Proveedor { get; set; } = "openai"; // openai | anthropic | azure
    public string ApiKey { get; set; } = "";
    public string Modelo { get; set; } = "gpt-4o-mini";
    public string SystemPrompt { get; set; } = "";
}
