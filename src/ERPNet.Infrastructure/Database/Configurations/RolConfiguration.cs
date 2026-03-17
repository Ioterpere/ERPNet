using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.HasIndex(r => r.Nombre).IsUnique();
        builder.Property(r => r.Nombre).HasMaxLength(100);
        builder.Property(r => r.Descripcion).HasMaxLength(500);
    }
}
