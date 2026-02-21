#nullable enable
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Billing;

public class BillingEndpoint(IChannel channel)
{
    public async Task StartAsync(Func<string, string, Task>? onEventReceived = null)
    {
        await channel.QueueDeclareAsync(
            queue: "billing",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Declare the exchange we subscribe to (idempotent — Sales also declares it)
        await channel.ExchangeDeclareAsync(
            exchange: "order.accepted",
            durable: true,
            type: "topic");

        // Declare the exchange we publish to
        await channel.ExchangeDeclareAsync(
            exchange: "order.authorized",
            durable: true,
            type: "topic");

        await channel.ExchangeDeclareAsync(
            exchange: "order.not.authorized",
            durable: true,
            type: "topic");

        await channel.QueueBindAsync(
            queue: "billing",
            exchange: "order.accepted",
            routingKey: "order.accepted");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var receivedMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
            var receivedProps = ea.BasicProperties;

            if (onEventReceived != null)
                await onEventReceived(receivedMessage, receivedProps.CorrelationId!);

            var eventProps = new BasicProperties { CorrelationId = receivedProps.CorrelationId };
            var eventMessage = $"Payment authorized for order Order {receivedProps.CorrelationId}";

            await channel.BasicPublishAsync(
                exchange: "order.authorized",
                routingKey: "order.authorized",
                mandatory: false,
                basicProperties: eventProps,
                body: Encoding.UTF8.GetBytes(eventMessage));
        };

        await channel.BasicConsumeAsync(queue: "billing", autoAck: true, consumer: consumer);
    }
}
