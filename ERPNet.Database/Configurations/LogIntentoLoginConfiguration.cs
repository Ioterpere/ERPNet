using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class LogIntentoLoginConfiguration : IEntityTypeConfiguration<LogIntentoLogin>
{
    public void Configure(EntityTypeBuilder<LogIntentoLogin> builder)
    {
        builder.Property(l => l.NombreUsuario).HasMaxLength(256);
        builder.Property(l => l.DireccionIp).HasMaxLength(45);

        builder.HasIndex(l => new { l.NombreUsuario, l.FechaIntento });
        builder.HasIndex(l => new { l.DireccionIp, l.FechaIntento });

        builder.HasOne(l => l.Usuario)
            .WithMany(u => u.IntentosLogin)
            .HasForeignKey(l => l.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
