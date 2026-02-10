namespace ERPNet.Domain.Common;

public abstract class StaticEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
}
