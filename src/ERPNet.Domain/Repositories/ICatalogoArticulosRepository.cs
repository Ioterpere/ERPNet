using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface ICatalogoArticulosRepository
{
    Task<List<TipoIva>> GetTiposIvaAsync();
    Task<List<FormatoArticulo>> GetFormatosAsync();
    Task<List<ConfiguracionCaducidad>> GetConfiguracionesCaducidadAsync();
}
