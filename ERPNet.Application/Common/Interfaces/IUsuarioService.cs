using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IUsuarioService
{
    Task<Result<ListaPaginada<UsuarioResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<UsuarioResponse>> GetByIdAsync(int id);
    Task<Result<UsuarioResponse>> GetMeAsync(UsuarioContext usuario);
    Task<Result<UsuarioResponse>> CreateAsync(CreateUsuarioRequest request);
    Task<Result> UpdateAsync(int id, UpdateUsuarioRequest request);
    Task<Result> DeleteAsync(int id);
    Task<Result> CambiarContrasenaAsync(int usuarioId, CambiarContrasenaRequest request);
    Task<Result> ResetearContrasenaAsync(int usuarioId);
    Task<Result> AsignarRolesAsync(int usuarioId, AsignarRolesRequest request, int? empresaId = null);
    Task<Result<List<AsignacionRolDto>>> GetTodasAsignacionesRolAsync(int usuarioId);
    Task<Result> SincronizarTodasAsignacionesRolAsync(int usuarioId, List<AsignacionRolDto> asignaciones);
}
