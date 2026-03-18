namespace ERPNet.Domain.Entities;

public class ConfiguracionCaducidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int DiasAviso { get; set; }
}
