using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class MarcajeConfiguration : IEntityTypeConfiguration<Marcaje>
{
    public void Configure(EntityTypeBuilder<Marcaje> builder)
    {
        builder.HasOne(m => m.Empleado)
            .WithMany(e => e.Marcajes)
            .HasForeignKey(m => m.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
