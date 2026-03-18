using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class ArticuloMappings
{
    public static ArticuloResponse ToResponse(this Articulo articulo) => new()
    {
        Id                          = articulo.Id,
        Codigo                      = articulo.Codigo,
        Descripcion                 = articulo.Descripcion,
        UnidadMedida                = articulo.UnidadMedida,
        PrecioCompra                = articulo.PrecioCompra,
        PrecioVenta                 = articulo.PrecioVenta,
        Activo                      = articulo.Activo,
        EmpresaId                   = articulo.EmpresaId,
        FamiliaArticuloId           = articulo.FamiliaArticuloId,
        FamiliaArticuloNombre       = articulo.FamiliaArticulo?.Nombre,
        TipoIvaId                   = articulo.TipoIvaId,
        TipoIvaNombre               = articulo.TipoIva?.Nombre,
        FormatoArticuloId           = articulo.FormatoArticuloId,
        FormatoArticuloNombre       = articulo.FormatoArticulo?.Nombre,
        ConfiguracionCaducidadId    = articulo.ConfiguracionCaducidadId,
        ConfiguracionCaducidadNombre = articulo.ConfiguracionCaducidad?.Nombre,
    };

    public static Articulo ToEntity(this CreateArticuloRequest request, int empresaId) => new()
    {
        Codigo                   = request.Codigo,
        Descripcion              = request.Descripcion,
        UnidadMedida             = request.UnidadMedida,
        PrecioCompra             = request.PrecioCompra,
        PrecioVenta              = request.PrecioVenta,
        Activo                   = true,
        EmpresaId                = empresaId,
        FamiliaArticuloId        = request.FamiliaArticuloId,
        TipoIvaId                = request.TipoIvaId,
        FormatoArticuloId        = request.FormatoArticuloId,
        ConfiguracionCaducidadId = request.ConfiguracionCaducidadId,
    };

    public static void ApplyTo(this UpdateArticuloRequest request, Articulo articulo)
    {
        if (request.Codigo is not null)
            articulo.Codigo = request.Codigo;

        if (request.Descripcion is not null)
            articulo.Descripcion = request.Descripcion;

        if (request.UnidadMedida is not null)
            articulo.UnidadMedida = request.UnidadMedida;

        if (request.PrecioCompra.HasValue)
            articulo.PrecioCompra = request.PrecioCompra.Value;

        if (request.PrecioVenta.HasValue)
            articulo.PrecioVenta = request.PrecioVenta.Value;

        if (request.Activo.HasValue)
            articulo.Activo = request.Activo.Value;

        if (request.FamiliaArticuloId.HasValue)
            articulo.FamiliaArticuloId = request.FamiliaArticuloId.Value == 0 ? null : request.FamiliaArticuloId;

        if (request.TipoIvaId.HasValue)
            articulo.TipoIvaId = request.TipoIvaId.Value == 0 ? null : request.TipoIvaId;

        if (request.FormatoArticuloId.HasValue)
            articulo.FormatoArticuloId = request.FormatoArticuloId.Value == 0 ? null : request.FormatoArticuloId;

        if (request.ConfiguracionCaducidadId.HasValue)
            articulo.ConfiguracionCaducidadId = request.ConfiguracionCaducidadId.Value == 0 ? null : request.ConfiguracionCaducidadId;
    }

    public static ArticuloLogResponse ToResponse(this ArticuloLog log) => new()
    {
        Id            = log.Id,
        ArticuloId    = log.ArticuloId,
        UsuarioId     = log.UsuarioId,
        UsuarioNombre = log.Usuario?.Empleado is not null
            ? $"{log.Usuario.Empleado.Nombre} {log.Usuario.Empleado.Apellidos}"
            : log.Usuario?.Email.ToString(),
        Fecha         = log.Fecha,
        Nota          = log.Nota,
        StockAnterior = log.StockAnterior,
        StockNuevo    = log.StockNuevo,
        CreatedAt     = log.CreatedAt,
    };

    public static FamiliaArticuloResponse ToResponse(this FamiliaArticulo familia) => new()
    {
        Id                = familia.Id,
        Nombre            = familia.Nombre,
        Descripcion       = familia.Descripcion,
        EmpresaId         = familia.EmpresaId,
        FamiliaPadreId    = familia.FamiliaPadreId,
        FamiliaPadreNombre = familia.FamiliaPadre?.Nombre,
    };

    public static TipoIvaResponse ToResponse(this TipoIva tipoIva) => new()
    {
        Id         = tipoIva.Id,
        Nombre     = tipoIva.Nombre,
        Porcentaje = tipoIva.Porcentaje,
    };

    public static FormatoArticuloResponse ToResponse(this FormatoArticulo formato) => new()
    {
        Id     = formato.Id,
        Nombre = formato.Nombre,
    };
}
