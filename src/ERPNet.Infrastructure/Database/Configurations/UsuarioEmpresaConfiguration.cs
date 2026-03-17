using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class UsuarioEmpresaConfiguration : IEntityTypeConfiguration<UsuarioEmpresa>
{
    public void Configure(EntityTypeBuilder<UsuarioEmpresa> builder)
    {
        builder.HasKey(ue => new { ue.UsuarioId, ue.EmpresaId });

        builder.HasOne(ue => ue.Usuario)
            .WithMany(u => u.UsuarioEmpresas)
            .HasForeignKey(ue => ue.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ue => ue.Empresa)
            .WithMany(e => e.UsuarioEmpresas)
            .HasForeignKey(ue => ue.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
