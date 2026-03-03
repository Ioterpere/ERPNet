namespace ERPNet.Application.Common.DTOs;

public record AsignacionUsuarioDto
{
    public int UsuarioId { get; init; }
    public int? EmpresaId { get; init; }
}
