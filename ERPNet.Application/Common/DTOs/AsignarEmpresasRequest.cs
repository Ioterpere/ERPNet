namespace ERPNet.Application.Common.DTOs;

public record AsignarEmpresasRequest
{
    public List<int> EmpresaIds { get; init; } = [];
}
