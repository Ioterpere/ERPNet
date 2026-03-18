using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class RolUsuarioConfiguration : IEntityTypeConfiguration<RolUsuario>
{
    public void Configure(EntityTypeBuilder<RolUsuario> builder)
    {
        builder.HasKey(ru => ru.Id);

        builder.HasOne(ru => ru.Usuario)
            .WithMany(u => u.RolesUsuarios)
            .HasForeignKey(ru => ru.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ru => ru.Rol)
            .WithMany(r => r.RolesUsuarios)
            .HasForeignKey(ru => ru.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ru => ru.Empresa)
            .WithMany()
            .HasForeignKey(ru => ru.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unicidad: un usuario no puede tener el mismo rol global dos veces
        builder.HasIndex(ru => new { ru.UsuarioId, ru.RolId })
            .IsUnique()
            .HasFilter("[EmpresaId] IS NULL")
            .HasDatabaseName("IX_RolUsuario_UsuarioId_RolId_Global");

        // Unicidad: un usuario no puede tener el mismo rol en la misma empresa dos veces
        builder.HasIndex(ru => new { ru.UsuarioId, ru.RolId, ru.EmpresaId })
            .IsUnique()
            .HasFilter("[EmpresaId] IS NOT NULL")
            .HasDatabaseName("IX_RolUsuario_UsuarioId_RolId_Empresa");
    }
}
