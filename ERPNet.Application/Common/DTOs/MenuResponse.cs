namespace ERPNet.Application.Common.DTOs;

public class MenuResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Path { get; set; }
    public string? IconClass { get; set; }
    public string? CustomClass { get; set; }
    public int Orden { get; set; }
    public List<MenuResponse> SubMenus { get; set; } = [];
}
