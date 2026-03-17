using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ERPNet.Infrastructure.Messaging;

public class RabbitMqConnectionProvider(
    IOptions<MessageBrokerSettings> options,
    ILogger<RabbitMqConnectionProvider> logger) : IHostedService, IAsyncDisposable
{
    private IConnection? _connection;
    private readonly TaskCompletionSource<IConnection> _ready = new();

    public Task<IConnection> GetConnectionAsync() => _ready.Task;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost
        };

        const int maxRetries = 10;
        for (var i = 1; i <= maxRetries; i++)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _ready.SetResult(_connection);
                logger.LogInformation("Conexion a RabbitMQ establecida");
                return;
            }
            catch (Exception ex) when (i < maxRetries)
            {
                logger.LogWarning("RabbitMQ no disponible (intento {Intento}/{Max}): {Mensaje}", i, maxRetries, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2 * i), cancellationToken);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
            await _connection.CloseAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}
