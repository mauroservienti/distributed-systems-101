using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Billing;

class Program
{
    public static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await channel.QueueDeclareAsync(queue: "billing",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.ExchangeDeclareAsync(exchange: "order.authorized",
            durable: true,
            type: "topic");
        await channel.ExchangeDeclareAsync(exchange: "order.not.authorized",
            durable: true,
            type: "topic");

        await channel.QueueBindAsync(queue: "billing",
            exchange: "order.accepted",
            routingKey: "order.accepted");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var receivedBody = ea.Body.ToArray();
            var receivedMessage = Encoding.UTF8.GetString(receivedBody);
            var receivedProps = ea.BasicProperties;
            Console.WriteLine($"Received {receivedMessage} with correlation ID {receivedProps.CorrelationId}");
            Console.WriteLine($"Attempt to authorize customer credit card for order {receivedProps.CorrelationId}");

            var eventProps = new BasicProperties
            {
                CorrelationId = receivedProps.CorrelationId
            };

            var eventMessage = $"Payment authorized for order Order {receivedProps.CorrelationId}";
            var eventBody = Encoding.UTF8.GetBytes(eventMessage);

            await channel.BasicPublishAsync(
                "order.authorized",
                "order.authorized",
                true,
                eventProps,
                eventBody);

            Console.WriteLine($"Published {eventMessage}");
        };

        await channel.BasicConsumeAsync(queue: "billing",
            autoAck: true,
            consumer: consumer);

        Console.WriteLine(" Billing endpoint running.");
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}