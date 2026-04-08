using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class ApunteContableConfiguration : IEntityTypeConfiguration<ApunteContable>
{
    public void Configure(EntityTypeBuilder<ApunteContable> builder)
    {
        builder.ToTable("diario");

        builder.HasIndex(a => new { a.Asiento, a.NumLinea, a.EmpresaId }).IsUnique();
        builder.HasIndex(a => new { a.CuentaId, a.Fecha });
        builder.HasIndex(a => a.Fecha);

        builder.Property(a => a.Concepto).HasMaxLength(40);
        builder.Property(a => a.Debe).HasPrecision(10, 2);
        builder.Property(a => a.Haber).HasPrecision(10, 2);

        builder.HasOne(a => a.Cuenta)
            .WithMany(c => c.Apuntes)
            .HasForeignKey(a => a.CuentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TipoDiario)
            .WithMany(t => t.Apuntes)
            .HasForeignKey(a => a.TipoDiarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.CentroCoste)
            .WithMany(c => c.Apuntes)
            .HasForeignKey(a => a.CentroCosteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
