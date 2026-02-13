using System.Threading.Channels;
using ERPNet.Application.Email;

namespace ERPNet.Infrastructure.Email;

public class EmailChannel
{
    private readonly Channel<MensajeEmail> _channel =
        Channel.CreateUnbounded<MensajeEmail>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

    public ChannelReader<MensajeEmail> Reader => _channel.Reader;

    public ValueTask EscribirAsync(MensajeEmail mensaje, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(mensaje, ct);
}
