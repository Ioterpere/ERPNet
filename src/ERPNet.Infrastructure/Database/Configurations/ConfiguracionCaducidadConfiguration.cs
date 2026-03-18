using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class ConfiguracionCaducidadConfiguration : IEntityTypeConfiguration<ConfiguracionCaducidad>
{
    public void Configure(EntityTypeBuilder<ConfiguracionCaducidad> builder)
    {
        builder.ToTable("configuraciones_caducidad");

        builder.Property(c => c.Nombre).HasMaxLength(100);

        builder.HasData(
            new ConfiguracionCaducidad { Id = 1, Nombre = "7 días antes",   DiasAviso = 7  },
            new ConfiguracionCaducidad { Id = 2, Nombre = "15 días antes",  DiasAviso = 15 },
            new ConfiguracionCaducidad { Id = 3, Nombre = "30 días antes",  DiasAviso = 30 },
            new ConfiguracionCaducidad { Id = 4, Nombre = "60 días antes",  DiasAviso = 60 },
            new ConfiguracionCaducidad { Id = 5, Nombre = "90 días antes",  DiasAviso = 90 }
        );
    }
}
