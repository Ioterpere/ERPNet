using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class ArticuloLogConfiguration : IEntityTypeConfiguration<ArticuloLog>
{
    public void Configure(EntityTypeBuilder<ArticuloLog> builder)
    {
        builder.ToTable("articulos_log");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.Nota).HasMaxLength(1000);
        builder.Property(l => l.StockAnterior).HasPrecision(18, 4);
        builder.Property(l => l.StockNuevo).HasPrecision(18, 4);

        builder.HasOne(l => l.Articulo)
            .WithMany(a => a.Logs)
            .HasForeignKey(l => l.ArticuloId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Usuario)
            .WithMany()
            .HasForeignKey(l => l.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
