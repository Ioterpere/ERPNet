namespace ERPNet.Application.Auth.Interfaces;

public interface ICurrentUserProvider
{
    UsuarioContext? Current { get; }
}
