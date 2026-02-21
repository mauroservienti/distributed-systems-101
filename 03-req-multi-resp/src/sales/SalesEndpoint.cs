#nullable enable
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Sales;

public class SalesEndpoint(IChannel channel, TimeSpan? processingDelay = null)
{
    readonly TimeSpan _processingDelay = processingDelay ?? TimeSpan.FromSeconds(2);

    public async Task StartAsync(Func<string, string, Task>? onOrderReceived = null)
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
            var receivedMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
            var receivedProps = ea.BasicProperties;

            // First reply: order acknowledged
            var replyProps = new BasicProperties { CorrelationId = receivedProps.CorrelationId };
            var replyMessage = $"Order {receivedProps.CorrelationId} on its way...";

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: receivedProps.ReplyTo!,
                mandatory: true,
                basicProperties: replyProps,
                body: Encoding.UTF8.GetBytes(replyMessage));

            // Simulate work (e.g., persisting to a database and a batch job picking it up)
            await Task.Delay(_processingDelay);

            // Second reply: order shipped
            var shippedProps = new BasicProperties { CorrelationId = receivedProps.CorrelationId };
            var shippedMessage = $"Order {receivedProps.CorrelationId} shipped";

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: receivedProps.ReplyTo!,
                mandatory: true,
                basicProperties: shippedProps,
                body: Encoding.UTF8.GetBytes(shippedMessage));

            if (onOrderReceived != null)
                await onOrderReceived(receivedMessage, receivedProps.CorrelationId!);
        };

        await channel.BasicConsumeAsync(queue: "sales", autoAck: true, consumer: consumer);
    }
}
