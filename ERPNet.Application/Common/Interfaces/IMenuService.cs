using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Enums;

namespace ERPNet.Application.Common.Interfaces;

public interface IMenuService
{
    Task<Result<List<MenuResponse>>> GetMenusVisiblesAsync(Plataforma plataforma, List<int> rolIds);
    Task<Result<MenuResponse>> GetByIdAsync(int id);
    Task<Result<MenuResponse>> CreateAsync(CreateMenuRequest request);
    Task<Result<List<int>>> GetRolesAsync(int menuId);
    Task<Result> AsignarRolesAsync(int menuId, AsignarRolesRequest request);
}
