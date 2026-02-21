#nullable enable
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Sales;

public class SalesEndpoint(IChannel channel)
{
    public async Task StartAsync(Func<string, string, Task>? onOrderReceived = null)
    {
        await channel.QueueDeclareAsync(
            queue: "sales",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.ExchangeDeclareAsync(
            exchange: "order.accepted",
            durable: true,
            type: "topic");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var receivedMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
            var receivedProps = ea.BasicProperties;

            // Reply to the requester
            var replyProps = new BasicProperties { CorrelationId = receivedProps.CorrelationId };
            var replyMessage = $"Order {receivedProps.CorrelationId} on its way...";

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: receivedProps.ReplyTo!,
                mandatory: true,
                basicProperties: replyProps,
                body: Encoding.UTF8.GetBytes(replyMessage));

            // Publish event so subscribers (e.g. Billing, Warehouse) can react
            var eventProps = new BasicProperties { CorrelationId = receivedProps.CorrelationId };
            var eventMessage = $"Order {receivedProps.CorrelationId} accepted";

            await channel.BasicPublishAsync(
                exchange: "order.accepted",
                routingKey: "order.accepted",
                mandatory: false,
                basicProperties: eventProps,
                body: Encoding.UTF8.GetBytes(eventMessage));

            if (onOrderReceived != null)
                await onOrderReceived(receivedMessage, receivedProps.CorrelationId!);
        };

        await channel.BasicConsumeAsync(queue: "sales", autoAck: true, consumer: consumer);
    }
}
