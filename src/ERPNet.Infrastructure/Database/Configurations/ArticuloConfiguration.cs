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

        // Identificación
        builder.Property(a => a.Codigo).HasMaxLength(50);
        builder.Property(a => a.CodigoBarras).HasMaxLength(13);

        // Descripción
        builder.Property(a => a.Descripcion).HasMaxLength(300);
        builder.Property(a => a.DescripcionVenta).HasMaxLength(300);

        // Unidades
        builder.Property(a => a.UnidadMedida).HasMaxLength(20);
        builder.Property(a => a.UnidadMedida2).HasMaxLength(3);

        // Precios
        builder.Property(a => a.PrecioCoste).HasPrecision(18, 4);
        builder.Property(a => a.PrecioMedio).HasPrecision(18, 4);
        builder.Property(a => a.PrecioVenta).HasPrecision(18, 4);

        // Stock
        builder.Property(a => a.StockMinimo).HasPrecision(18, 3);
        builder.Property(a => a.NivelPedido).HasPrecision(18, 3);
        builder.Property(a => a.NivelReposicion).HasPrecision(18, 3);
        builder.Property(a => a.UnidadesCaja).HasPrecision(18, 3);
        builder.Property(a => a.PesoGramo).HasPrecision(18, 3);
        builder.Property(a => a.Depreciacion).HasPrecision(5, 2);

        // Texto libre
        builder.Property(a => a.ProveedorPrincipal).HasMaxLength(200);
        builder.Property(a => a.Observaciones).HasMaxLength(2000);

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
