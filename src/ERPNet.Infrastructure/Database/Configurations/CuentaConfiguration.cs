using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("cuentas");

        builder.HasIndex(c => new { c.Codigo, c.EmpresaId }).IsUnique();

        builder.Property(c => c.Codigo).HasMaxLength(9);
        builder.Property(c => c.Descripcion).HasMaxLength(60);
        builder.Property(c => c.DescripcionSII).HasMaxLength(60);
        builder.Property(c => c.Nif).HasMaxLength(15);

        builder.HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CuentaPadre)
            .WithMany(c => c.CuentasHijas)
            .HasForeignKey(c => c.CuentaPadreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CuentaAmortizacion)
            .WithMany()
            .HasForeignKey(c => c.CuentaAmortizacionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.CuentaPagoDelegado)
            .WithMany()
            .HasForeignKey(c => c.CuentaPagoDelegadoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.EmpresaVinculada)
            .WithMany()
            .HasForeignKey(c => c.EmpresaVinculadaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
