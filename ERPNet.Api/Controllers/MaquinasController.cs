using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Contracts;
using ERPNet.Contracts.DTOs;
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
    [ProducesResponseType<ListaPaginada<MaquinariaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await maquinariaService.GetAllAsync(filtro));

    [HttpGet("{id}")]
    [ProducesResponseType<MaquinariaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await maquinariaService.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<MaquinariaResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMaquinariaRequest request)
        => CreatedFromResult(await maquinariaService.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMaquinariaRequest request)
        => FromResult(await maquinariaService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await maquinariaService.DeleteAsync(id));
}
