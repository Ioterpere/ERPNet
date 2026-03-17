using ERPNet.Application.Common.DTOs;

namespace ERPNet.Application.Common.Interfaces;

public interface ISeccionService
{
    Task<Result<List<SeccionResponse>>> GetAllAsync();
}
