namespace ERPNet.ApiClient;

/// <summary>
/// Representa un archivo para subir en peticiones multipart/form-data.
/// Requerido por el código generado por NSwag para endpoints con IFormFile.
/// </summary>
public sealed class FileParameter
{
    public FileParameter(System.IO.Stream data, string? fileName = null, string? contentType = null)
    {
        Data = data;
        FileName = fileName;
        ContentType = contentType;
    }

    public System.IO.Stream Data { get; }
    public string? FileName { get; }
    public string? ContentType { get; }
}
