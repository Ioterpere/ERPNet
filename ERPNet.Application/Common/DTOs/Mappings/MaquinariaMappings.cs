using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class MaquinariaMappings
{
    public static MaquinariaResponse ToResponse(this Maquinaria maquinaria) => new()
    {
        Id = maquinaria.Id,
        Nombre = maquinaria.Nombre,
        Codigo = maquinaria.Codigo,
        Ubicacion = maquinaria.Ubicacion,
        Activa = maquinaria.Activa,
        SeccionId = maquinaria.SeccionId,
        FichaTecnicaId = maquinaria.FichaTecnicaId,
        ManualId = maquinaria.ManualId,
        CertificadoCeId = maquinaria.CertificadoCeId,
        FotoId = maquinaria.FotoId
    };

    public static Maquinaria ToEntity(this CreateMaquinariaRequest request) => new()
    {
        Nombre = request.Nombre,
        Codigo = request.Codigo,
        Ubicacion = request.Ubicacion,
        Activa = true,
        SeccionId = request.SeccionId
    };

    public static void ApplyTo(this UpdateMaquinariaRequest request, Maquinaria maquinaria)
    {
        if (request.Nombre is not null)
            maquinaria.Nombre = request.Nombre;

        if (request.Codigo is not null)
            maquinaria.Codigo = request.Codigo;

        if (request.Ubicacion is not null)
            maquinaria.Ubicacion = request.Ubicacion;

        if (request.Activa.HasValue)
            maquinaria.Activa = request.Activa.Value;

        if (request.SeccionId.HasValue)
            maquinaria.SeccionId = request.SeccionId.Value;
    }
}
