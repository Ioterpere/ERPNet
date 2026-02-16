namespace ERPNet.Web.Blazor.Auth;

internal sealed record TokenData(string AccessToken, string RefreshToken, DateTime Expiry);
