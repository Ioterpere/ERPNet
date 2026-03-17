using System.Text.RegularExpressions;

namespace ERPNet.Domain.Common.Values;

public readonly partial record struct Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var trimmed = value.Trim().ToLowerInvariant();

        if (trimmed.Length > 256)
            throw new ArgumentException("El email no puede tener más de 256 caracteres.", nameof(value));

        if (!EmailRegex().IsMatch(trimmed))
            throw new ArgumentException("El formato del email no es válido.", nameof(value));

        return new Email(trimmed);
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
