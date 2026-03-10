using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Enums;

namespace ERPNet.Application.Common.Interfaces;

public interface IMenuService
{
    Task<Result<List<MenuResponse>>> GetMenusVisiblesAsync(Plataforma plataforma, List<int> rolIds);
    Task<Result<List<MenuResponse>>> GetAllAdminAsync(Plataforma plataforma);
    Task<Result<MenuResponse>> GetByIdAsync(int id);
    Task<Result<MenuResponse>> CreateAsync(CreateMenuRequest request);
    Task<Result<MenuResponse>> UpdateAsync(int id, UpdateMenuRequest request);
    Task<Result> DeleteAsync(int id);
    Task<Result> MoverAsync(int id, MoverMenuRequest request);
    Task<Result<List<int>>> GetRolesAsync(int menuId);
    Task<Result> AsignarRolesAsync(int menuId, AsignarRolesRequest request);
}
