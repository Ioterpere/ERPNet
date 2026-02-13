namespace ERPNet.Application.Mailing;

public class EmailSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; } = false;
    public string? Usuario { get; set; }
    public string? Password { get; set; }
    public string RemitenteEmail { get; set; } = "noreply@erpnet.local";
    public string RemitenteNombre { get; set; } = "ERPNet";
}
