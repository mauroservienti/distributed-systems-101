using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace Website;

class Program
{
    public static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        await channel.QueueDeclareAsync(queue: "website",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            var receivedBody = ea.Body.ToArray();
            var receivedMessage = Encoding.UTF8.GetString(receivedBody);
            Console.WriteLine($"Received {receivedMessage} with correlation ID {ea.BasicProperties.CorrelationId}");

            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queue: "website",
            autoAck: true,
            consumer: consumer);

        Console.WriteLine("Endpoint ready, press [enter] to send a message.");
        Console.Read();

        var props = new BasicProperties
        {
            ReplyTo = "website",
            CorrelationId = "order-abc"
        };

        const string message = "Hello World!";
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            "",
            "sales",
            true,
            basicProperties: props,
            body: body);

        Console.WriteLine($" Sent {message}");
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}