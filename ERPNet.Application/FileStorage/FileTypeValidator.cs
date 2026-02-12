namespace ERPNet.Application.FileStorage;

public static class FileTypeValidator
{
    private static readonly Dictionary<string, TipoPermitido> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"]  = new("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".jpeg"] = new("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".png"]  = new("image/png", [0x89, 0x50, 0x4E, 0x47]),
        [".gif"]  = new("image/gif", [0x47, 0x49, 0x46, 0x38]),
        [".webp"] = new("image/webp", null), // validacion especial RIFF+WEBP
        [".bmp"]  = new("image/bmp", [0x42, 0x4D]),
        [".pdf"]  = new("application/pdf", [0x25, 0x50, 0x44, 0x46]),
        [".docx"] = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document", [0x50, 0x4B, 0x03, 0x04]),
        [".xlsx"] = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", [0x50, 0x4B, 0x03, 0x04]),
        [".pptx"] = new("application/vnd.openxmlformats-officedocument.presentationml.presentation", [0x50, 0x4B, 0x03, 0x04]),
        [".csv"]  = new("text/csv", null),
        [".txt"]  = new("text/plain", null),
    };

    // Extensiones que requieren validacion de magic bytes
    private static readonly HashSet<string> RequiereMagicBytes = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".pdf", ".docx", ".xlsx", ".pptx"
    };

    public static (bool EsValido, string? ContentType, string? Error) Validar(string nombreArchivo, Stream contenido)
    {
        var extension = Path.GetExtension(nombreArchivo);

        if (string.IsNullOrEmpty(extension) || !TiposPermitidos.TryGetValue(extension, out var tipo))
        {
            return (false, null, $"Tipo de archivo no permitido: '{extension}'. " +
                $"Extensiones permitidas: {string.Join(", ", TiposPermitidos.Keys.Distinct(StringComparer.OrdinalIgnoreCase).Order())}.");
        }

        if (!RequiereMagicBytes.Contains(extension))
            return (true, tipo.ContentType, null);

        // Validar magic bytes
        Span<byte> buffer = stackalloc byte[12];
        var posicionOriginal = contenido.Position;
        contenido.Position = 0;
        var bytesLeidos = contenido.Read(buffer);
        contenido.Position = posicionOriginal;

        if (bytesLeidos < 4)
            return (false, null, "El archivo está vacío o es demasiado pequeño para validar su tipo.");

        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            // RIFF (bytes 0-3) + WEBP (bytes 8-11)
            ReadOnlySpan<byte> riff = [0x52, 0x49, 0x46, 0x46];
            ReadOnlySpan<byte> webp = [(byte)'W', (byte)'E', (byte)'B', (byte)'P'];

            if (bytesLeidos < 12 || !buffer[..4].SequenceEqual(riff) || !buffer[8..12].SequenceEqual(webp))
                return (false, null, "El contenido del archivo no corresponde a un archivo WebP válido.");
        }
        else if (tipo.MagicBytes is not null)
        {
            ReadOnlySpan<byte> expected = tipo.MagicBytes;
            if (bytesLeidos < expected.Length || !buffer[..expected.Length].SequenceEqual(expected))
                return (false, null, $"El contenido del archivo no corresponde a un archivo {extension} válido.");
        }

        return (true, tipo.ContentType, null);
    }

    private sealed record TipoPermitido(string ContentType, byte[]? MagicBytes);
}
