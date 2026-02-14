using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Repositories;
using ERPNet.Application.Common;
using ERPNet.Application.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using ERPNet.Application.Common.DTOs;
using ERPNet.Api.Attributes;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Auth.DTOs.Mappings;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class UsuariosController(
    IUsuarioRepository usuarioRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
    {
        var (usuarios, total) = await usuarioRepository.GetPaginatedAsync(filtro);
        var response = usuarios.Select(u => u.ToResponse()).ToList();
        return FromResult(Result<ListaPaginada<UsuarioResponse>>.Success(
            ListaPaginada<UsuarioResponse>.Crear(response, total, filtro)));
    }

    [SinPermiso]
    [HttpGet("account")]
    public IActionResult GetMe()
    {
        return FromResult(Result<AccountResponse>.Success(UsuarioActual.ToResponse()));
    }

    [HttpGet("{id}", Name = nameof(GetById))]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id);

        if (usuario is null)
            return FromResult(Result.Failure("Usuario no encontrado.", ErrorType.NotFound));

        return FromResult(Result<UsuarioResponse>.Success(usuario.ToResponse()));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
    {
        var usuario = request.ToEntity(BCrypt.Net.BCrypt.HashPassword(request.Password));

        await usuarioRepository.CreateAsync(usuario);
        await unitOfWork.SaveChangesAsync();

        return CreatedFromResult(
            Result<UsuarioResponse>.Success(usuario.ToResponse()),
            nameof(GetById),
            new { id = usuario.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id);

        if (usuario is null)
            return FromResult(Result.Failure("Usuario no encontrado.", ErrorType.NotFound));

        if (request.Email is not null && request.Email != usuario.Email.Value)
        {
            if (await usuarioRepository.ExisteEmailAsync(request.Email, id))
                return FromResult(Result.Failure("Ya existe un usuario con ese email.", ErrorType.Conflict));
        }

        request.ApplyTo(usuario);
        await unitOfWork.SaveChangesAsync();
        cache.Remove($"usuario:{id}");

        return FromResult(Result.Success());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id);

        if (usuario is null)
            return FromResult(Result.Failure("Usuario no encontrado.", ErrorType.NotFound));

        usuarioRepository.Delete(usuario);
        await unitOfWork.SaveChangesAsync();
        cache.Remove($"usuario:{id}");

        return FromResult(Result.Success());
    }
}
