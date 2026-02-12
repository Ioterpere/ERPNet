using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class ArchivoConfiguration : IEntityTypeConfiguration<Archivo>
{
    public void Configure(EntityTypeBuilder<Archivo> builder)
    {
        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.Property(a => a.NombreOriginal).HasMaxLength(500);
        builder.Property(a => a.ContentType).HasMaxLength(100);

        builder.HasOne(a => a.ArchivoOriginal)
            .WithMany(a => a.Thumbnails)
            .HasForeignKey(a => a.ArchivoOriginalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
