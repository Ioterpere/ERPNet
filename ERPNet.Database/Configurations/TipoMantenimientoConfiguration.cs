using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class TipoMantenimientoConfiguration : IEntityTypeConfiguration<TipoMantenimiento>
{
    public void Configure(EntityTypeBuilder<TipoMantenimiento> builder)
    {
        builder.Property(t => t.Nombre).HasMaxLength(100);
    }
}
