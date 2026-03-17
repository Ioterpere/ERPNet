using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class EmpresaMappings
{
    public static EmpresaResponse ToResponse(this Empresa empresa) => new()
    {
        Id = empresa.Id,
        Nombre = empresa.Nombre,
        Cif = empresa.Cif,
        Activo = empresa.Activo
    };

    public static Empresa ToEntity(this CreateEmpresaRequest request) => new()
    {
        Nombre = request.Nombre,
        Cif = request.Cif,
        Activo = request.Activo
    };

    public static void ApplyTo(this UpdateEmpresaRequest request, Empresa empresa)
    {
        empresa.Nombre = request.Nombre;
        empresa.Cif = request.Cif;
        empresa.Activo = request.Activo;
    }
}
