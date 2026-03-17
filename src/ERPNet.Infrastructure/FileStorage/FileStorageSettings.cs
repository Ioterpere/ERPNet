namespace ERPNet.Infrastructure.FileStorage;

public class FileStorageSettings
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string BucketName { get; set; } = "erpnet";
    public bool UseSSL { get; set; }
    public int ThumbnailSize { get; set; } = 300;
    public long MaxFileSize { get; set; } = 20_971_520; // 20 MB
}
