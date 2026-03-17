namespace ERPNet.Application.Common.DTOs;

public record RecursoResponse
{
    public int Id { get; init; }
    public required string Codigo { get; init; }
}
