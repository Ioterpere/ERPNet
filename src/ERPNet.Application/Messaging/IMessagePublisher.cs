namespace ERPNet.Application.Messaging;

public interface IMessagePublisher<in T> where T : class
{
    Task PublicarAsync(T mensaje, CancellationToken ct = default);
}
