using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class TipoIvaConfiguration : IEntityTypeConfiguration<TipoIva>
{
    public void Configure(EntityTypeBuilder<TipoIva> builder)
    {
        builder.ToTable("tipos_iva");

        builder.Property(t => t.Nombre).HasMaxLength(50);
        builder.Property(t => t.Porcentaje).HasPrecision(5, 2);

        builder.HasData(
            new TipoIva { Id = 1, Nombre = "IVA 0%",   Porcentaje = 0m   },
            new TipoIva { Id = 2, Nombre = "IVA 4%",   Porcentaje = 4m   },
            new TipoIva { Id = 3, Nombre = "IVA 10%",  Porcentaje = 10m  },
            new TipoIva { Id = 4, Nombre = "IVA 21%",  Porcentaje = 21m  }
        );
    }
}
