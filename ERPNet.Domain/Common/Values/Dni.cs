using System.Text.RegularExpressions;

namespace ERPNet.Domain.Common.Values;

public readonly partial record struct Dni
{
    private static readonly char[] LetrasDni = "TRWAGMYFPDXBNJZSQVHLCKE".ToCharArray();

    public string Value { get; }

    private Dni(string value) => Value = value;

    public static Dni From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var normalizado = value.Trim().ToUpperInvariant();

        if (!DniRegex().IsMatch(normalizado))
            throw new ArgumentException("El formato del DNI no es válido. Debe ser 8 dígitos seguidos de una letra.", nameof(value));

        var numero = int.Parse(normalizado[..8]);
        var letraEsperada = LetrasDni[numero % 23];

        if (normalizado[8] != letraEsperada)
            throw new ArgumentException($"La letra del DNI no es correcta. Se esperaba '{letraEsperada}'.", nameof(value));

        return new Dni(normalizado);
    }

    public static implicit operator string(Dni dni) => dni.Value;
    public override string ToString() => Value;

    [GeneratedRegex(@"^\d{8}[A-Z]$")]
    private static partial Regex DniRegex();
}
