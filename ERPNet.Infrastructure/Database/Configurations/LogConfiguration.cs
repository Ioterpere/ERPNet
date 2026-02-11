using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.Property(l => l.Accion).HasMaxLength(100);
        builder.Property(l => l.Entidad).HasMaxLength(100);
        builder.Property(l => l.EntidadId).HasMaxLength(50);
        builder.Property(l => l.Detalle).HasMaxLength(2000);
        builder.Property(l => l.CodigoError).HasMaxLength(50);

        builder.HasOne(l => l.Usuario)
            .WithMany(u => u.Logs)
            .HasForeignKey(l => l.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
