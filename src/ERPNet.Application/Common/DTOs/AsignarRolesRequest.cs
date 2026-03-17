namespace ERPNet.Application.Common.DTOs;

public record AsignarRolesRequest
{
    public List<int> RolIds { get; init; } = [];
}
