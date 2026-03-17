namespace ERPNet.Application.Common.DTOs;

public record UpdateEmpresaRequest
{
    public required string Nombre { get; init; }
    public string? Cif { get; init; }
    public bool Activo { get; init; }
}
