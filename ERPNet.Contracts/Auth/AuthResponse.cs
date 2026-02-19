namespace ERPNet.Contracts.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public bool RequiereCambioContrasena { get; set; }
}
