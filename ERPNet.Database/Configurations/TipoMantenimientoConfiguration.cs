using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class TipoMantenimientoConfiguration : IEntityTypeConfiguration<TipoMantenimiento>
{
    public void Configure(EntityTypeBuilder<TipoMantenimiento> builder)
    {
        builder.Property(t => t.Codigo).HasMaxLength(200);

        builder.HasData(
            Enum.GetValues<TipoMantenimientoCodigo>()
                .Select(c => new { Id = (int)c, Codigo = c.ToString() }));

    }
}
