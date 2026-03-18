using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class ArticuloConfiguration : IEntityTypeConfiguration<Articulo>
{
    public void Configure(EntityTypeBuilder<Articulo> builder)
    {
        builder.ToTable("articulos");

        builder.HasIndex(a => new { a.Codigo, a.EmpresaId }).IsUnique();
        builder.Property(a => a.Codigo).HasMaxLength(50);
        builder.Property(a => a.Descripcion).HasMaxLength(300);
        builder.Property(a => a.UnidadMedida).HasMaxLength(20);
        builder.Property(a => a.PrecioCompra).HasPrecision(18, 4);
        builder.Property(a => a.PrecioVenta).HasPrecision(18, 4);

        builder.HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.FamiliaArticulo)
            .WithMany(f => f.Articulos)
            .HasForeignKey(a => a.FamiliaArticuloId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.TipoIva)
            .WithMany()
            .HasForeignKey(a => a.TipoIvaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.FormatoArticulo)
            .WithMany()
            .HasForeignKey(a => a.FormatoArticuloId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.ConfiguracionCaducidad)
            .WithMany()
            .HasForeignKey(a => a.ConfiguracionCaducidadId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
