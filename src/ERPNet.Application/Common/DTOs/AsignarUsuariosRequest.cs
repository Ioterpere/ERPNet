namespace ERPNet.Application.Common.DTOs;

public record AsignarUsuariosRequest
{
    public List<int> UsuarioIds { get; init; } = [];
}
