using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.Property(r => r.Codigo).HasMaxLength(50);
        builder.HasIndex(r => r.Codigo).IsUnique();

        builder.Property(r => r.Descripcion).HasMaxLength(500);
    }
}
