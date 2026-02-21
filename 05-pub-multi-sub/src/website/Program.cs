using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Website;

class Program
{
    public static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        var endpoint = new WebsiteEndpoint(channel);
        await endpoint.StartAsync((message, correlationId) =>
        {
            Console.WriteLine($"Received {message} with correlation ID {correlationId}");
            return Task.CompletedTask;
        });

        Console.WriteLine("Endpoint ready, press [enter] to send a message.");
        Console.Read();

        const string message = "Hello World!";
        await endpoint.SendOrderAsync(message, "order-abc");
        Console.WriteLine($" Sent {message}");

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}