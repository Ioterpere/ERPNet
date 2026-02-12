using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class ArchivoRepository(ERPNetDbContext context) : IArchivoRepository
{
    public async Task<Archivo?> GetByIdAsync(Guid id)
    {
        return await context.Archivos
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Archivo?> GetByIdConThumbnailsAsync(Guid id)
    {
        return await context.Archivos
            .Include(a => a.Thumbnails)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Archivo> CreateAsync(Archivo archivo)
    {
        await context.Archivos.AddAsync(archivo);
        return archivo;
    }
}
