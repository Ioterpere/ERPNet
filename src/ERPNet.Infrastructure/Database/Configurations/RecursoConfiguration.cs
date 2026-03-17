using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.Property(r => r.Codigo).HasMaxLength(200);

        builder.HasData(
            Enum.GetValues<RecursoCodigo>()
                .Select(c => new { Id = (int)c, Codigo = c.ToString() }));
    }
}
