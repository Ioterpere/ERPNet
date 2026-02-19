using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.FileStorage;
using ERPNet.Contracts.FileStorage;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class ArchivosController(
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork) : BaseController
{
    [HttpPost]
    [ProducesResponseType<ArchivoResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Subir(IFormFile archivo, CancellationToken ct)
    {
        if (archivo.Length == 0)
            return BadRequest(new { error = "El archivo está vacío." });

        using var stream = archivo.OpenReadStream();
        var result = await fileStorage.SubirAsync(stream, archivo.FileName, archivo.ContentType, ct);

        if (!result.IsSuccess)
            return FromResult(result);

        await unitOfWork.SaveChangesAsync(ct);

        return CreatedFromResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Descargar(Guid id, CancellationToken ct)
    {
        var result = await fileStorage.DescargarAsync(id, ct);
        return await DescargarArchivo(result, ct);
    }

    [HttpGet("{id:guid}/thumbnail")]
    public async Task<IActionResult> DescargarThumbnail(Guid id, CancellationToken ct)
    {
        var result = await fileStorage.DescargarThumbnailAsync(id, ct);
        return await DescargarArchivo(result, ct);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var result = await fileStorage.EliminarAsync(id, ct);

        if (!result.IsSuccess)
            return FromResult(result);

        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
