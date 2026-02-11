using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Interfaces;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Common;
using ERPNet.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERPNet.Infrastructure.Database;

public class UnitOfWork(ERPNetDbContext context, ICurrentUserProvider currentUser, ILogService logService) : IUnitOfWork
{
    private static readonly HashSet<string> CamposAuditoria =
    [
        nameof(BaseEntity.CreatedAt),
        nameof(BaseEntity.CreatedBy),
        nameof(BaseEntity.UpdatedAt),
        nameof(BaseEntity.UpdatedBy),
        nameof(BaseEntity.IsDeleted),
        nameof(BaseEntity.DeletedBy),
        nameof(BaseEntity.DeletedAt)
    ];

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        context.ChangeTracker.DetectChanges();

        var usuarioId = currentUser.Current?.Id;
        var ahora = DateTime.UtcNow;
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = ahora;
                    entry.Entity.CreatedBy = usuarioId;
                    auditEntries.Add(new AuditEntry(entry, "Crear"));
                    break;

                case EntityState.Modified when IsSoftDelete(entry):
                    entry.Entity.DeletedAt = ahora;
                    entry.Entity.DeletedBy = usuarioId;
                    auditEntries.Add(new AuditEntry(entry, "Eliminar"));
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = ahora;
                    entry.Entity.UpdatedBy = usuarioId;
                    auditEntries.Add(new AuditEntry(entry, "Editar"));
                    break;
            }
        }

        await context.SaveChangesAsync(ct);

        if (auditEntries.Count > 0)
        {
            CreateLogs(auditEntries, usuarioId);
            await context.SaveChangesAsync(ct);
        }
    }

    private static bool IsSoftDelete(EntityEntry<BaseEntity> entry)
    {
        var prop = entry.Property(nameof(BaseEntity.IsDeleted));
        return prop.IsModified && (bool)prop.CurrentValue! == true;
    }

    private void CreateLogs(List<AuditEntry> entries, int? usuarioId)
    {
        foreach (var (entry, accion) in entries)
        {
            logService.Entidad(
                accion,
                entry.Metadata.ClrType.Name,
                entry.Entity.Id.ToString(),
                usuarioId,
                accion == "Editar" ? BuildModifiedDetail(entry) : null);
        }
    }

    private static string? BuildModifiedDetail(EntityEntry<BaseEntity> entry)
    {
        var changes = new List<string>();

        foreach (var prop in entry.Properties)
        {
            if (!prop.IsModified || CamposAuditoria.Contains(prop.Metadata.Name))
                continue;

            var original = prop.OriginalValue?.ToString() ?? "null";
            var current = prop.CurrentValue?.ToString() ?? "null";
            changes.Add($"{prop.Metadata.Name}: {original} → {current}");
        }

        if (changes.Count == 0)
            return null;

        var detail = string.Join("; ", changes);
        return detail.Length > 2000 ? detail[..2000] : detail;
    }

    private record AuditEntry(EntityEntry<BaseEntity> Entry, string Accion);
}
