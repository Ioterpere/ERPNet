using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class TipoDiarioConfiguration : IEntityTypeConfiguration<TipoDiario>
{
    public void Configure(EntityTypeBuilder<TipoDiario> builder)
    {
        builder.ToTable("tiposdiario");

        builder.HasIndex(t => new { t.Codigo, t.EmpresaId }).IsUnique();

        builder.Property(t => t.Codigo).HasMaxLength(2);
        builder.Property(t => t.Descripcion).HasMaxLength(25);

        builder.HasOne(t => t.Empresa)
            .WithMany()
            .HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
