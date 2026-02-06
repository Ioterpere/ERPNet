using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class SeccionConfiguration : IEntityTypeConfiguration<Seccion>
{
    public void Configure(EntityTypeBuilder<Seccion> builder)
    {
        builder.Property(s => s.Nombre).HasMaxLength(200);

        builder.HasOne(s => s.Responsable)
            .WithMany()
            .HasForeignKey(s => s.ResponsableId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
