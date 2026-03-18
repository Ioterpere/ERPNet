using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class FormatoArticuloConfiguration : IEntityTypeConfiguration<FormatoArticulo>
{
    public void Configure(EntityTypeBuilder<FormatoArticulo> builder)
    {
        builder.ToTable("formatos_articulo");

        builder.Property(f => f.Nombre).HasMaxLength(50);

        builder.HasData(
            new FormatoArticulo { Id = 1, Nombre = "Unidad"      },
            new FormatoArticulo { Id = 2, Nombre = "Caja"        },
            new FormatoArticulo { Id = 3, Nombre = "Palet"       },
            new FormatoArticulo { Id = 4, Nombre = "Kilogramo"   },
            new FormatoArticulo { Id = 5, Nombre = "Litro"       }
        );
    }
}
