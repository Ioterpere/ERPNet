namespace ERPNet.Application.Common.DTOs;

public record CreateEmpresaRequest
{
    public required string Nombre { get; init; }
    public string? Cif { get; init; }
    public bool Activo { get; init; } = true;
}
