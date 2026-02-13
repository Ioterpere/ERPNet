using ERPNet.Application.Common;
using ERPNet.Application.FileStorage;
using ERPNet.Application.FileStorage.DTOs;
using ERPNet.Domain.Common;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers.Common;

public abstract class ArchivoBaseController<TEntidad, TEnum>(
    IFileStorageService fileStorage,
    IRepository<TEntidad> repo,
    IUnitOfWork unitOfWork) : BaseController
    where TEntidad : BaseEntity, IHasArchivos<TEnum>
    where TEnum : struct, Enum
{
    [HttpPut("{id:int}/archivos/{campo}")]
    public async Task<IActionResult> SubirArchivo(
        int id, string campo, IFormFile archivo, CancellationToken ct)
    {
        if (!TryParseCampo(campo, out var campoEnum))
            return NotFound();

        if (archivo.Length == 0)
            return BadRequest(new { error = "El archivo está vacío." });

        var entidad = await repo.GetByIdAsync(id);
        if (entidad is null)
            return NotFound();

        if (!entidad.AceptaContentType(campoEnum, archivo.ContentType))
            return BadRequest(new { error = "Tipo de archivo no permitido para este campo." });

        var slotActual = entidad.GetArchivoId(campoEnum);
        if (slotActual is not null)
        {
            var eliminar = await fileStorage.EliminarAsync(slotActual.Value, ct);
            if (!eliminar.IsSuccess)
                return FromResult(eliminar);
        }

        using var stream = archivo.OpenReadStream();
        var result = await fileStorage.SubirAsync(stream, archivo.FileName, archivo.ContentType, ct);
        if (!result.IsSuccess)
            return FromResult(result);

        entidad.SetArchivoId(campoEnum, result.Value!.Id);
        await unitOfWork.SaveChangesAsync(ct);

        return Ok(result.Value);
    }

    [HttpGet("{id:int}/archivos/{campo}")]
    public async Task<IActionResult> DescargarArchivoEntidad(
        int id, string campo, [FromQuery] bool thumbnail, CancellationToken ct)
    {
        if (!TryParseCampo(campo, out var campoEnum))
            return NotFound();

        var entidad = await repo.GetByIdAsync(id);
        if (entidad is null)
            return NotFound();

        var slotId = entidad.GetArchivoId(campoEnum);
        if (slotId is null)
            return NotFound(new { error = $"No tiene {campo}." });

        Result<ArchivoDescarga> result = thumbnail
            ? await fileStorage.DescargarThumbnailAsync(slotId.Value, ct)
            : await fileStorage.DescargarAsync(slotId.Value, ct);

        return await DescargarArchivo(result, ct);
    }

    [HttpDelete("{id:int}/archivos/{campo}")]
    public async Task<IActionResult> EliminarArchivo(
        int id, string campo, CancellationToken ct)
    {
        if (!TryParseCampo(campo, out var campoEnum))
            return NotFound();

        var entidad = await repo.GetByIdAsync(id);
        if (entidad is null)
            return NotFound();

        var slotId = entidad.GetArchivoId(campoEnum);
        if (slotId is null)
            return NotFound(new { error = $"No tiene {campo}." });

        var result = await fileStorage.EliminarAsync(slotId.Value, ct);
        if (!result.IsSuccess)
            return FromResult(result);

        entidad.SetArchivoId(campoEnum, null);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    private static bool TryParseCampo(string campo, out TEnum result)
    {
        var pascalCase = string.Concat(
            campo.Split('-').Select(s =>
                s.Length > 0 ? char.ToUpperInvariant(s[0]) + s[1..] : s));

        return Enum.TryParse(pascalCase, ignoreCase: true, out result);
    }
}
