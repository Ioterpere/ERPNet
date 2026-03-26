using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class ArticuloMappings
{
    public static ArticuloResponse ToResponse(this Articulo articulo) => new()
    {
        Id                           = articulo.Id,
        Codigo                       = articulo.Codigo,
        CodigoBarras                 = articulo.CodigoBarras,
        Descripcion                  = articulo.Descripcion,
        DescripcionVenta             = articulo.DescripcionVenta,
        UnidadMedida                 = articulo.UnidadMedida,
        UnidadMedida2                = articulo.UnidadMedida2,
        PrecioCoste                  = articulo.PrecioCoste,
        PrecioMedio                  = articulo.PrecioMedio,
        PrecioVenta                  = articulo.PrecioVenta,
        StockMinimo                  = articulo.StockMinimo,
        NivelPedido                  = articulo.NivelPedido,
        NivelReposicion              = articulo.NivelReposicion,
        UnidadesCaja                 = articulo.UnidadesCaja,
        UnidadesPalet                = articulo.UnidadesPalet,
        FilasPalet                   = articulo.FilasPalet,
        PesoGramo                    = articulo.PesoGramo,
        LeadTime                     = articulo.LeadTime,
        DiasVidaUtil                 = articulo.DiasVidaUtil,
        Depreciacion                 = articulo.Depreciacion,
        EsInventariable              = articulo.EsInventariable,
        EsPropio                     = articulo.EsPropio,
        EsNuevo                      = articulo.EsNuevo,
        EsObsoleto                   = articulo.EsObsoleto,
        ProveedorPrincipal           = articulo.ProveedorPrincipal,
        Observaciones                = articulo.Observaciones,
        EmpresaId                    = articulo.EmpresaId,
        FamiliaArticuloId            = articulo.FamiliaArticuloId,
        FamiliaArticuloNombre        = articulo.FamiliaArticulo?.Nombre,
        TipoIvaId                    = articulo.TipoIvaId,
        TipoIvaNombre                = articulo.TipoIva?.Nombre,
        FormatoArticuloId            = articulo.FormatoArticuloId,
        FormatoArticuloNombre        = articulo.FormatoArticulo?.Nombre,
        ConfiguracionCaducidadId     = articulo.ConfiguracionCaducidadId,
        ConfiguracionCaducidadNombre = articulo.ConfiguracionCaducidad?.Nombre,
    };

    public static Articulo ToEntity(this CreateArticuloRequest request, int empresaId) => new()
    {
        Codigo            = request.Codigo,
        Descripcion       = request.Descripcion,
        UnidadMedida      = request.UnidadMedida,
        EmpresaId         = empresaId,
        FamiliaArticuloId = request.FamiliaArticuloId,
        EsInventariable   = request.EsInventariable,
        EsPropio          = true,
    };

    public static void ApplyTo(this UpdateArticuloRequest request, Articulo articulo)
    {
        if (request.Codigo is not null)
            articulo.Codigo = request.Codigo;

        if (request.CodigoBarras is not null)
            articulo.CodigoBarras = string.IsNullOrWhiteSpace(request.CodigoBarras) ? null : request.CodigoBarras;

        if (request.Descripcion is not null)
            articulo.Descripcion = request.Descripcion;

        if (request.DescripcionVenta is not null)
            articulo.DescripcionVenta = string.IsNullOrWhiteSpace(request.DescripcionVenta) ? null : request.DescripcionVenta;

        if (request.UnidadMedida is not null)
            articulo.UnidadMedida = string.IsNullOrWhiteSpace(request.UnidadMedida) ? null : request.UnidadMedida;

        if (request.UnidadMedida2 is not null)
            articulo.UnidadMedida2 = string.IsNullOrWhiteSpace(request.UnidadMedida2) ? null : request.UnidadMedida2;

        if (request.PrecioCoste.HasValue)
            articulo.PrecioCoste = request.PrecioCoste.Value;

        if (request.PrecioMedio.HasValue)
            articulo.PrecioMedio = request.PrecioMedio.Value;

        if (request.PrecioVenta.HasValue)
            articulo.PrecioVenta = request.PrecioVenta.Value;

        if (request.StockMinimo.HasValue)
            articulo.StockMinimo = request.StockMinimo.Value;

        if (request.NivelPedido.HasValue)
            articulo.NivelPedido = request.NivelPedido.Value;

        if (request.NivelReposicion.HasValue)
            articulo.NivelReposicion = request.NivelReposicion.Value;

        if (request.UnidadesCaja.HasValue)
            articulo.UnidadesCaja = request.UnidadesCaja.Value;

        if (request.UnidadesPalet.HasValue)
            articulo.UnidadesPalet = request.UnidadesPalet.Value;

        if (request.FilasPalet.HasValue)
            articulo.FilasPalet = request.FilasPalet.Value;

        if (request.PesoGramo.HasValue)
            articulo.PesoGramo = request.PesoGramo.Value;

        if (request.LeadTime.HasValue)
            articulo.LeadTime = request.LeadTime.Value == 0 ? null : request.LeadTime;

        if (request.DiasVidaUtil.HasValue)
            articulo.DiasVidaUtil = request.DiasVidaUtil.Value;

        if (request.Depreciacion.HasValue)
            articulo.Depreciacion = request.Depreciacion.Value;

        if (request.EsInventariable.HasValue)
            articulo.EsInventariable = request.EsInventariable.Value;

        if (request.EsPropio.HasValue)
            articulo.EsPropio = request.EsPropio.Value;

        if (request.EsNuevo.HasValue)
            articulo.EsNuevo = request.EsNuevo.Value;

        if (request.EsObsoleto.HasValue)
            articulo.EsObsoleto = request.EsObsoleto.Value;

        if (request.ProveedorPrincipal is not null)
            articulo.ProveedorPrincipal = string.IsNullOrWhiteSpace(request.ProveedorPrincipal) ? null : request.ProveedorPrincipal;

        if (request.Observaciones is not null)
            articulo.Observaciones = string.IsNullOrWhiteSpace(request.Observaciones) ? null : request.Observaciones;

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
        Id                 = familia.Id,
        Nombre             = familia.Nombre,
        Descripcion        = familia.Descripcion,
        EmpresaId          = familia.EmpresaId,
        FamiliaPadreId     = familia.FamiliaPadreId,
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

    public static ConfiguracionCaducidadResponse ToResponse(this ConfiguracionCaducidad c) => new()
    {
        Id       = c.Id,
        Nombre   = c.Nombre,
        DiasAviso = c.DiasAviso,
    };
}
