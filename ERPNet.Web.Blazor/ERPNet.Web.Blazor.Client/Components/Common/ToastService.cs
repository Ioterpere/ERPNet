namespace ERPNet.Web.Blazor.Client;

public enum ToastTipo { Exito, Error, Aviso, Info }

public record ToastItem(Guid Id, string Mensaje, ToastTipo Tipo, int DuracionMs);

/// <summary>
/// Servicio scoped para mostrar notificaciones toast desde cualquier componente.
/// Uso: inyectar <see cref="ToastService"/> y llamar a Exito/Error/Aviso/Info.
/// </summary>
public class ToastService
{
    public event Action<ToastItem>? OnToastAdded;

    public void Mostrar(string mensaje, ToastTipo tipo = ToastTipo.Info, int duracionMs = 4000)
        => OnToastAdded?.Invoke(new ToastItem(Guid.NewGuid(), mensaje, tipo, duracionMs));

    public void Exito(string mensaje, int duracionMs = 4000) => Mostrar(mensaje, ToastTipo.Exito, duracionMs);
    public void Error(string mensaje, int duracionMs = 5000) => Mostrar(mensaje, ToastTipo.Error, duracionMs);
    public void Aviso(string mensaje, int duracionMs = 4500) => Mostrar(mensaje, ToastTipo.Aviso, duracionMs);
    public void Info(string mensaje, int duracionMs = 4000)  => Mostrar(mensaje, ToastTipo.Info,  duracionMs);
}
