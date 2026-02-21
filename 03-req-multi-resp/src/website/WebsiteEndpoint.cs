#nullable enable
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Website;

public class WebsiteEndpoint(IChannel channel)
{
    public async Task StartAsync(Func<string, string, Task>? onReplyReceived = null)
    {
        await channel.QueueDeclareAsync(
            queue: "website",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var correlationId = ea.BasicProperties.CorrelationId;
            if (onReplyReceived != null)
                await onReplyReceived(message, correlationId!);
        };

        await channel.BasicConsumeAsync(queue: "website", autoAck: true, consumer: consumer);
    }

    public async Task SendOrderAsync(string message, string correlationId)
    {
        var props = new BasicProperties
        {
            ReplyTo = "website",
            CorrelationId = correlationId
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "sales",
            mandatory: true,
            basicProperties: props,
            body: Encoding.UTF8.GetBytes(message));
    }
}
