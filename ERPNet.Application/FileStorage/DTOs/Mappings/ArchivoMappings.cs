using ERPNet.Contracts.FileStorage;
using ERPNet.Domain.Entities;

namespace ERPNet.Application.FileStorage.DTOs.Mappings;

public static class ArchivoMappings
{
    public static ArchivoResponse ToResponse(this Archivo archivo) => new()
    {
        Id = archivo.Id,
        NombreOriginal = archivo.NombreOriginal,
        ContentType = archivo.ContentType,
        Tamaño = archivo.Tamaño,
        EsThumbnail = archivo.EsThumbnail,
        ThumbnailId = archivo.Thumbnails.FirstOrDefault()?.Id,
        CreatedAt = archivo.CreatedAt
    };
}
