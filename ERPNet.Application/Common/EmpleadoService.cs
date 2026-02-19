using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class EmpleadoService(
    IEmpleadoRepository empleadoRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserProvider currentUser) : IEmpleadoService
{
    public async Task<Result<ListaPaginada<EmpleadoResponse>>> GetAllAsync(PaginacionFilter filtro)
    {
        var usuario = currentUser.Current!;
        var alcance = ObtenerAlcance();

        var (empleados, total) = await empleadoRepository.GetPaginatedAsync(
            filtro, alcance, usuario.EmpleadoId, usuario.SeccionId);

        var response = empleados.Select(e => e.ToResponse()).ToList();
        return Result<ListaPaginada<EmpleadoResponse>>.Success(
            ListaPaginada<EmpleadoResponse>.Crear(response, total, filtro));
    }

    public async Task<Result<EmpleadoResponse>> GetByIdAsync(int id)
    {
        var empleado = await empleadoRepository.GetByIdAsync(id);

        if (empleado is null)
            return Result<EmpleadoResponse>.Failure("Empleado no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(empleado))
            return Result<EmpleadoResponse>.Failure("No tiene acceso a este empleado.", ErrorType.Forbidden);

        return Result<EmpleadoResponse>.Success(empleado.ToResponse());
    }

    public async Task<Result<EmpleadoResponse>> GetMeAsync()
    {
        var empleadoId = currentUser.Current!.EmpleadoId;
        var empleado = await empleadoRepository.GetByIdAsync(empleadoId);

        if (empleado is null)
            return Result<EmpleadoResponse>.Failure("Empleado no encontrado.", ErrorType.NotFound);

        return Result<EmpleadoResponse>.Success(empleado.ToResponse());
    }

    public async Task<Result<EmpleadoResponse>> CreateAsync(CreateEmpleadoRequest request)
    {
        if (await empleadoRepository.ExisteDniAsync(request.Dni))
            return Result<EmpleadoResponse>.Failure("Ya existe un empleado con ese DNI.", ErrorType.Conflict);

        var empleado = request.ToEntity();

        await empleadoRepository.CreateAsync(empleado);
        await unitOfWork.SaveChangesAsync();

        return Result<EmpleadoResponse>.Success(empleado.ToResponse());
    }

    public async Task<Result> UpdateAsync(int id, UpdateEmpleadoRequest request)
    {
        var empleado = await empleadoRepository.GetByIdAsync(id);

        if (empleado is null)
            return Result.Failure("Empleado no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(empleado))
            return Result.Failure("No tiene acceso a este empleado.", ErrorType.Forbidden);

        if (request.Dni is not null && request.Dni != empleado.DNI.Value)
        {
            if (await empleadoRepository.ExisteDniAsync(request.Dni, id))
                return Result.Failure("Ya existe un empleado con ese DNI.", ErrorType.Conflict);
        }

        request.ApplyTo(empleado);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var empleado = await empleadoRepository.GetByIdAsync(id);

        if (empleado is null)
            return Result.Failure("Empleado no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(empleado))
            return Result.Failure("No tiene acceso a este empleado.", ErrorType.Forbidden);

        empleadoRepository.Delete(empleado);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    private Alcance ObtenerAlcance()
    {
        var permiso = currentUser.Current!.Permisos
            .FirstOrDefault(p => p.Codigo == RecursoCodigo.Empleados);
        return permiso?.Alcance ?? Alcance.Propio;
    }

    private bool TieneAcceso(Empleado empleado)
    {
        var usuario = currentUser.Current!;
        return ObtenerAlcance() switch
        {
            Alcance.Propio => empleado.Id == usuario.EmpleadoId,
            Alcance.Seccion => empleado.SeccionId == usuario.SeccionId,
            _ => true
        };
    }
}
