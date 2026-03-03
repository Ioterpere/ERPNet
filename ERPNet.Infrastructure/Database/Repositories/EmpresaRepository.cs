using System.Linq.Expressions;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPNet.Infrastructure.Database.Repositories;

public class EmpresaRepository(ERPNetDbContext context, ICurrentUserProvider currentUser) : Repository<Empresa>(context, currentUser), IEmpresaRepository
{
    protected override Expression<Func<Empresa, bool>>? GetBusquedaPredicate(string busqueda)
        => e => e.Nombre.Contains(busqueda);

    public async Task<List<Empresa>> GetEmpresasDeUsuarioAsync(int usuarioId)
    {
        return await Context.UsuarioEmpresas
            .Where(ue => ue.UsuarioId == usuarioId)
            .Select(ue => ue.Empresa)
            .OrderBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task SincronizarEmpresasDeUsuarioAsync(int usuarioId, List<int> empresaIds)
    {
        var empresaIdsSet = empresaIds.ToHashSet();

        // IgnoreQueryFilters para incluir registros soft-deleted (necesario para restaurarlos sin violar la PK)
        var actuales = await Context.UsuarioEmpresas
            .IgnoreQueryFilters()
            .Where(ue => ue.UsuarioId == usuarioId)
            .ToListAsync();

        // Cascade: eliminar roles de empresas que se desasocian (solo las que estaban activas)
        var aEliminar = actuales.Where(ue => !ue.IsDeleted && !empresaIdsSet.Contains(ue.EmpresaId)).ToList();
        if (aEliminar.Count > 0)
        {
            foreach (var ue in aEliminar)
                ue.IsDeleted = true;

            var empresasEliminadas = aEliminar.Select(ue => (int?)ue.EmpresaId).ToList();
            var rolesAEliminar = await Context.RolesUsuarios
                .Where(ru => ru.UsuarioId == usuarioId && empresasEliminadas.Contains(ru.EmpresaId))
                .ToListAsync();
            Context.RolesUsuarios.RemoveRange(rolesAEliminar);
        }

        // Restaurar registros soft-deleted que se re-añaden (evita violación de PK)
        foreach (var ue in actuales.Where(ue => ue.IsDeleted && empresaIdsSet.Contains(ue.EmpresaId)))
            ue.IsDeleted = false;

        // Crear registros genuinamente nuevos (nunca existieron)
        var existentesIds = actuales.Select(ue => ue.EmpresaId).ToHashSet();
        var aCrear = empresaIds
            .Where(eid => !existentesIds.Contains(eid))
            .Select(eid => new UsuarioEmpresa { UsuarioId = usuarioId, EmpresaId = eid });

        Context.UsuarioEmpresas.AddRange(aCrear);
    }
}
