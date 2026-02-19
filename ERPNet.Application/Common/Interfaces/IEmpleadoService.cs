using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IEmpleadoService
{
    Task<Result<ListaPaginada<EmpleadoResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<EmpleadoResponse>> GetByIdAsync(int id);
    Task<Result<EmpleadoResponse>> GetMeAsync();
    Task<Result<EmpleadoResponse>> CreateAsync(CreateEmpleadoRequest request);
    Task<Result> UpdateAsync(int id, UpdateEmpleadoRequest request);
    Task<Result> DeleteAsync(int id);
}
