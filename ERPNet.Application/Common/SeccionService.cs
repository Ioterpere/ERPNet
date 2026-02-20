using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class SeccionService(ISeccionRepository repo) : ISeccionService
{
    public async Task<Result<List<SeccionResponse>>> GetAllAsync()
    {
        var secciones = await repo.GetAllAsync();
        return Result<List<SeccionResponse>>.Success(secciones.Select(s => s.ToResponse()).ToList());
    }
}
