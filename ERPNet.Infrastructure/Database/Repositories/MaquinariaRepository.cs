using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class MaquinariaRepository(ERPNetDbContext context) : Repository<Maquinaria>(context), IMaquinariaRepository
{
    public override async Task<Maquinaria?> GetByIdAsync(int id)
    {
        return await Context.Maquinas
            .Include(m => m.FichaTecnica)
            .Include(m => m.Manual)
            .Include(m => m.CertificadoCe)
            .Include(m => m.Foto)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
