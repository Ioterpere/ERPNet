using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class EmpleadoRepository(ERPNetDbContext context) : Repository<Empleado>(context), IEmpleadoRepository
{
    public override async Task<Empleado?> GetByIdAsync(int id)
    {
        return await Context.Empleados
            .Include(e => e.Foto)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
