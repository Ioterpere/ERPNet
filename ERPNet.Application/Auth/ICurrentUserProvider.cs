namespace ERPNet.Application.Auth;

public interface ICurrentUserProvider
{
    UsuarioContext? Current { get; }
}
