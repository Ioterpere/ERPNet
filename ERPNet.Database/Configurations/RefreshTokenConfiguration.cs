using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.Token).HasMaxLength(512);
        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.ReemplazadoPor).HasMaxLength(512);

        builder.HasOne(rt => rt.Usuario)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
