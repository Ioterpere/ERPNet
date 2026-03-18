using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class CatalogoArticulosRepository(ERPNetDbContext context) : ICatalogoArticulosRepository
{
    public async Task<List<TipoIva>> GetTiposIvaAsync()
        => await context.TiposIva.AsNoTracking().OrderBy(t => t.Porcentaje).ToListAsync();

    public async Task<List<FormatoArticulo>> GetFormatosAsync()
        => await context.FormatosArticulo.AsNoTracking().OrderBy(f => f.Nombre).ToListAsync();

    public async Task<List<ConfiguracionCaducidad>> GetConfiguracionesCaducidadAsync()
        => await context.ConfiguracionesCaducidad.AsNoTracking().OrderBy(c => c.DiasAviso).ToListAsync();
}
