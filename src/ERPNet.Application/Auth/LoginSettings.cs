namespace ERPNet.Application.Auth;

public class LoginSettings
{
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int MaxFailedAttemptsPerIp { get; set; } = 20;
    public int IpLockoutMinutes { get; set; } = 30;
    public int IpWindowMinutes { get; set; } = 10;
}
