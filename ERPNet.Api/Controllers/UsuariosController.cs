using ERPNet.Application.DTOs.Mappings;
using ERPNet.Application.Interfaces;
using ERPNet.Domain.Repositories;
using ERPNet.Application;
using ERPNet.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using ERPNet.Application.DTOs;
using ERPNet.Api.Attributes;
using ERPNet.Domain.Enums;
using ERPNet.Api.Controllers.Common;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class UsuariosController(
    IUsuarioRepository usuarioRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await usuarioRepository.GetAllAsync();
        var response = usuarios.Select(u => u.ToResponse()).ToList();
        return FromResult(Result<List<UsuarioResponse>>.Success(response));
    }

    [SinPermiso]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var usuario = await usuarioRepository.GetByIdAsync(UsuarioActual.Id);

        if (usuario is null)
            return FromResult(Result.Failure("Usuario no encontrado.", ErrorType.NotFound));

        return FromResult(Result<UsuarioResponse>.Success(usuario.ToResponse()));
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
