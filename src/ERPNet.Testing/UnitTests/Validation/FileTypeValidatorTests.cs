using ERPNet.Application.FileStorage;
using Xunit;

namespace ERPNet.Testing.UnitTests.Validation;

public class FileTypeValidatorTests
{
    private static MemoryStream CrearStream(params byte[] bytes) => new(bytes);

    #region Extensiones válidas (magic bytes)

    [Fact(DisplayName = "Validar: JPG con magic bytes correctos es válido")]
    public void Validar_Jpg_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00);

        var (esValido, contentType, _) = FileTypeValidator.Validar("foto.jpg", stream);

        Assert.True(esValido);
        Assert.Equal("image/jpeg", contentType);
    }

    [Fact(DisplayName = "Validar: PNG con magic bytes correctos es válido")]
    public void Validar_Png_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);

        var (esValido, contentType, _) = FileTypeValidator.Validar("imagen.png", stream);

        Assert.True(esValido);
        Assert.Equal("image/png", contentType);
    }

    [Fact(DisplayName = "Validar: GIF con magic bytes correctos es válido")]
    public void Validar_Gif_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x00, 0x00);

        var (esValido, contentType, _) = FileTypeValidator.Validar("animacion.gif", stream);

        Assert.True(esValido);
        Assert.Equal("image/gif", contentType);
    }

    [Fact(DisplayName = "Validar: WebP completo (RIFF+WEBP) es válido")]
    public void Validar_Webp_Completo_EsValido()
    {
        // RIFF....WEBP
        using var stream = CrearStream(
            0x52, 0x49, 0x46, 0x46,  // RIFF
            0x00, 0x00, 0x00, 0x00,  // file size
            0x57, 0x45, 0x42, 0x50); // WEBP

        var (esValido, contentType, _) = FileTypeValidator.Validar("foto.webp", stream);

        Assert.True(esValido);
        Assert.Equal("image/webp", contentType);
    }

    [Fact(DisplayName = "Validar: BMP con magic bytes correctos es válido")]
    public void Validar_Bmp_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0x42, 0x4D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);

        var (esValido, contentType, _) = FileTypeValidator.Validar("imagen.bmp", stream);

        Assert.True(esValido);
        Assert.Equal("image/bmp", contentType);
    }

    [Fact(DisplayName = "Validar: PDF con magic bytes correctos es válido")]
    public void Validar_Pdf_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34);

        var (esValido, contentType, _) = FileTypeValidator.Validar("documento.pdf", stream);

        Assert.True(esValido);
        Assert.Equal("application/pdf", contentType);
    }

    [Fact(DisplayName = "Validar: DOCX (ZIP) con magic bytes correctos es válido")]
    public void Validar_Docx_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00);

        var (esValido, contentType, _) = FileTypeValidator.Validar("doc.docx", stream);

        Assert.True(esValido);
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", contentType);
    }

    [Fact(DisplayName = "Validar: XLSX (ZIP) con magic bytes correctos es válido")]
    public void Validar_Xlsx_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00);

        var (esValido, contentType, _) = FileTypeValidator.Validar("hoja.xlsx", stream);

        Assert.True(esValido);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", contentType);
    }

    [Fact(DisplayName = "Validar: PPTX (ZIP) con magic bytes correctos es válido")]
    public void Validar_Pptx_MagicBytesCorrecto_EsValido()
    {
        using var stream = CrearStream(0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00);

        var (esValido, contentType, _) = FileTypeValidator.Validar("presentacion.pptx", stream);

        Assert.True(esValido);
        Assert.Equal("application/vnd.openxmlformats-officedocument.presentationml.presentation", contentType);
    }

    #endregion

    #region Tipos texto (sin magic bytes)

    [Fact(DisplayName = "Validar: CSV aceptado sin validar contenido")]
    public void Validar_Csv_AceptadoSinMagicBytes()
    {
        using var stream = CrearStream(0x41, 0x42, 0x43); // "ABC"

        var (esValido, contentType, _) = FileTypeValidator.Validar("datos.csv", stream);

        Assert.True(esValido);
        Assert.Equal("text/csv", contentType);
    }

    [Fact(DisplayName = "Validar: TXT aceptado sin validar contenido")]
    public void Validar_Txt_AceptadoSinMagicBytes()
    {
        using var stream = CrearStream(0x48, 0x6F, 0x6C, 0x61); // "Hola"

        var (esValido, contentType, _) = FileTypeValidator.Validar("notas.txt", stream);

        Assert.True(esValido);
        Assert.Equal("text/plain", contentType);
    }

    #endregion

    #region Extensión rechazada

    [Fact(DisplayName = "Validar: extensión .exe rechazada")]
    public void Validar_Exe_Rechazado()
    {
        using var stream = CrearStream(0x4D, 0x5A, 0x00, 0x00);

        var (esValido, _, error) = FileTypeValidator.Validar("malware.exe", stream);

        Assert.False(esValido);
        Assert.Contains(".exe", error);
    }

    #endregion

    #region Sin extensión

    [Fact(DisplayName = "Validar: archivo sin extensión rechazado")]
    public void Validar_SinExtension_Rechazado()
    {
        using var stream = CrearStream(0x00, 0x00, 0x00, 0x00);

        var (esValido, _, error) = FileTypeValidator.Validar("archivo", stream);

        Assert.False(esValido);
        Assert.NotNull(error);
    }

    #endregion

    #region Magic bytes incorrectos

    [Fact(DisplayName = "Validar: extensión .png con bytes de .jpg rechazado")]
    public void Validar_PngConBytesJpg_Rechazado()
    {
        using var stream = CrearStream(0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00);

        var (esValido, _, error) = FileTypeValidator.Validar("imagen.png", stream);

        Assert.False(esValido);
        Assert.Contains(".png", error);
    }

    #endregion

    #region Stream muy pequeño

    [Fact(DisplayName = "Validar: stream de 2 bytes con extensión .png rechazado")]
    public void Validar_StreamMuyPequeño_Rechazado()
    {
        using var stream = CrearStream(0x89, 0x50);

        var (esValido, _, error) = FileTypeValidator.Validar("imagen.png", stream);

        Assert.False(esValido);
        Assert.Contains("pequeño", error);
    }

    #endregion

    #region Case insensitive

    [Fact(DisplayName = "Validar: extensión .PNG (mayúsculas) aceptada")]
    public void Validar_ExtensionMayusculas_Aceptada()
    {
        using var stream = CrearStream(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);

        var (esValido, contentType, _) = FileTypeValidator.Validar("IMAGEN.PNG", stream);

        Assert.True(esValido);
        Assert.Equal("image/png", contentType);
    }

    #endregion

    #region WebP incompleto

    [Fact(DisplayName = "Validar: WebP solo con RIFF sin WEBP rechazado")]
    public void Validar_WebpIncompleto_Rechazado()
    {
        // Solo RIFF, sin WEBP en bytes 8-11
        using var stream = CrearStream(
            0x52, 0x49, 0x46, 0x46,  // RIFF
            0x00, 0x00, 0x00, 0x00,  // file size
            0x41, 0x56, 0x49, 0x20); // AVI (no WEBP)

        var (esValido, _, error) = FileTypeValidator.Validar("foto.webp", stream);

        Assert.False(esValido);
        Assert.Contains("WebP", error);
    }

    #endregion
}
