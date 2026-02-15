using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IUsuarioService
{
    Task<Result<ListaPaginada<UsuarioResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<UsuarioResponse>> GetByIdAsync(int id);
    Task<Result<UsuarioResponse>> CreateAsync(CreateUsuarioRequest request);
    Task<Result> UpdateAsync(int id, UpdateUsuarioRequest request);
    Task<Result> DeleteAsync(int id);
    Task<Result> CambiarContrasenaAsync(int usuarioId, CambiarContrasenaRequest request);
    Task<Result> ResetearContrasenaAsync(int usuarioId, ResetearContrasenaRequest request);
}
