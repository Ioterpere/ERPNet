namespace ERPNet.Application.Common.DTOs;

public record EmpresaResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public string? Cif { get; init; }
    public bool Activo { get; init; }
}
