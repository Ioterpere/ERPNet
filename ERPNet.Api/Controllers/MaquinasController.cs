using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.FileStorage;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Maquinaria)]
public class MaquinasController(
    IFileStorageService fileStorage,
    IMaquinariaRepository repo,
    IUnitOfWork unitOfWork,
    IMaquinariaService maquinariaService)
    : ArchivoBaseController<Maquinaria, CampoArchivoMaquinaria>(fileStorage, repo, unitOfWork)
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await maquinariaService.GetAllAsync(filtro));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await maquinariaService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaquinariaRequest request)
        => CreatedFromResult(await maquinariaService.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMaquinariaRequest request)
        => FromResult(await maquinariaService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await maquinariaService.DeleteAsync(id));
}
