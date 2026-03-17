using System.Text.Json;
using ERPNet.Application.Messaging;
using RabbitMQ.Client;

namespace ERPNet.Infrastructure.Messaging;

public class RabbitMqPublisher<T>(RabbitMqConnectionProvider connectionProvider) : IMessagePublisher<T>, IAsyncDisposable
    where T : class
{
    private static readonly string QueueName = typeof(T).Name;

    private readonly Lazy<Task<IChannel>> _channel = new(async () =>
    {
        var connection = await connectionProvider.GetConnectionAsync();
        var ch = await connection.CreateChannelAsync();
        await QueueHelper.DeclararColaConDlqAsync(ch, QueueName);
        return ch;
    });

    public async Task PublicarAsync(T mensaje, CancellationToken ct = default)
    {
        var channel = await _channel.Value;

        var body = JsonSerializer.SerializeToUtf8Bytes(mensaje);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel.IsValueCreated)
        {
            var channel = await _channel.Value;
            await channel.CloseAsync();
            channel.Dispose();
        }
    }
}
