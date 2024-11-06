using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse;

class Program
{
    public static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await channel.QueueDeclareAsync(queue: "warehouse",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.ExchangeDeclareAsync(exchange: "order.items.collected",
            durable: true,
            type: "topic");

        await channel.QueueBindAsync(queue: "warehouse",
            exchange: "order.accepted",
            routingKey: "order.accepted");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var receivedBody = ea.Body.ToArray();
            var receivedMessage = Encoding.UTF8.GetString(receivedBody);
            var receivedProps = ea.BasicProperties;
            Console.WriteLine($"Received {receivedMessage} with correlation ID {receivedProps.CorrelationId}");
            Console.WriteLine($"Collecting items for order {receivedProps.CorrelationId}");

            var eventProps = new BasicProperties
            {
                CorrelationId = receivedProps.CorrelationId
            };

            string eventMessage = $"Items for Order {receivedProps.CorrelationId} collected";
            var eventBody = Encoding.UTF8.GetBytes(eventMessage);

            await channel.BasicPublishAsync(exchange: "order.items.collected",
                routingKey: "order.items.collected",
                true,
                basicProperties: eventProps,
                body: eventBody);

            Console.WriteLine($"Published {eventMessage}");
        };

        await channel.BasicConsumeAsync(queue: "warehouse",
            autoAck: true,
            consumer: consumer);

        Console.WriteLine(" Warehouse endpoint running.");
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}