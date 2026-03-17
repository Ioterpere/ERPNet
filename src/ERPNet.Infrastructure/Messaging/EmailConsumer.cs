using System.Text.Json;
using ERPNet.Application.Mailing;
using ERPNet.Application.Mailing.Models;
using ERPNet.Application.Messaging.Mensajes;
using ERPNet.Infrastructure.Mailing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ERPNet.Infrastructure.Messaging;

public class EmailConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailConsumer> logger) : BackgroundService
{
    private const string QueueName = nameof(EmailMensaje);
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await connectionProvider.GetConnectionAsync();
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await QueueHelper.DeclararColaConDlqAsync(_channel, QueueName, stoppingToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var mensaje = JsonSerializer.Deserialize<EmailMensaje>(ea.Body.Span)!;

                using var scope = scopeFactory.CreateScope();
                var renderer = scope.ServiceProvider.GetRequiredService<RazorViewToStringRenderer>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                object modelo = DeserializarModelo(mensaje.Plantilla, mensaje.Modelo);
                var html = await renderer.RenderAsync(mensaje.Plantilla, modelo);

                foreach (var destinatario in mensaje.Destinatarios)
                {
                    await emailSender.EnviarAsync(destinatario, mensaje.Asunto, html, stoppingToken);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error procesando email desde cola");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }

    private static object DeserializarModelo(string plantilla, JsonElement json) => plantilla switch
    {
        PlantillaEmail.Texto => json.Deserialize<TextoEmailModel>()!,
        PlantillaEmail.Bienvenida => json.Deserialize<BienvenidaEmailModel>()!,
        PlantillaEmail.Excepcion => json.Deserialize<ExcepcionEmailModel>()!,
        _ => throw new InvalidOperationException($"Plantilla desconocida: {plantilla}")
    };
}
