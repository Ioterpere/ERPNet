using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class CentroCosteConfiguration : IEntityTypeConfiguration<CentroCoste>
{
    public void Configure(EntityTypeBuilder<CentroCoste> builder)
    {
        builder.ToTable("centros");

        builder.HasIndex(c => new { c.Codigo, c.EmpresaId }).IsUnique();

        builder.Property(c => c.Codigo).HasMaxLength(4);
        builder.Property(c => c.Descripcion).HasMaxLength(50);

        builder.HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
