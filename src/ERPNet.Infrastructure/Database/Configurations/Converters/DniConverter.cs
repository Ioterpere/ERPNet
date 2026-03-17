using ERPNet.Domain.Common.Values;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ERPNet.Infrastructure.Database.Configurations.Converters;

public class DniConverter : ValueConverter<Dni, string>
{
    public DniConverter() : base(
        dni => dni.Value,
        value => Dni.From(value))
    {
    }
}
