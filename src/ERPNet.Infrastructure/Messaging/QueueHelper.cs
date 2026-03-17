using RabbitMQ.Client;

namespace ERPNet.Infrastructure.Messaging;

public static class QueueHelper
{
    private const string DlqSuffix = "_dlq";

    public static async Task DeclararColaConDlqAsync(
        IChannel channel, string queueName, CancellationToken ct = default)
    {
        var dlqName = queueName + DlqSuffix;

        await channel.QueueDeclareAsync(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "",
                ["x-dead-letter-routing-key"] = dlqName
            },
            cancellationToken: ct);
    }
}
