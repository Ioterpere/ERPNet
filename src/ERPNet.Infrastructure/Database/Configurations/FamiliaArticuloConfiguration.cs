using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class FamiliaArticuloConfiguration : IEntityTypeConfiguration<FamiliaArticulo>
{
    public void Configure(EntityTypeBuilder<FamiliaArticulo> builder)
    {
        builder.ToTable("familias_articulo");

        builder.Property(f => f.Nombre).HasMaxLength(100);
        builder.Property(f => f.Descripcion).HasMaxLength(500);

        builder.HasOne(f => f.Empresa)
            .WithMany()
            .HasForeignKey(f => f.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.FamiliaPadre)
            .WithMany(f => f.SubFamilias)
            .HasForeignKey(f => f.FamiliaPadreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
