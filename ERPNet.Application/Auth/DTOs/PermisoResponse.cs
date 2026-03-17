using ERPNet.Domain.Enums;

namespace ERPNet.Application.Auth.DTOs;

public record PermisoResponse
{
    public RecursoCodigo Codigo { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
    public Alcance Alcance { get; init; }
}
