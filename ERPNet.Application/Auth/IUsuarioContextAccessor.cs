namespace ERPNet.Application.Auth;

public interface IUsuarioContextAccessor
{
    UsuarioContext? Current { get; }
}
