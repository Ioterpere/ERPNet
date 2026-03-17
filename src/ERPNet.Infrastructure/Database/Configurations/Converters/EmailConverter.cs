using ERPNet.Domain.Common.Values;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ERPNet.Infrastructure.Database.Configurations.Converters;

public class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter() : base(
        email => email.Value,
        value => Email.From(value))
    {
    }
}
