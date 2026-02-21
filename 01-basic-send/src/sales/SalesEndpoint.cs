#nullable enable
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Sales;

public class SalesEndpoint(IChannel channel)
{
    public async Task StartAsync(Func<string, Task>? onMessageReceived = null)
    {
        await channel.QueueDeclareAsync(
            queue: "sales",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            if (onMessageReceived != null)
                await onMessageReceived(message);
        };

        await channel.BasicConsumeAsync(queue: "sales", autoAck: true, consumer: consumer);
    }
}
