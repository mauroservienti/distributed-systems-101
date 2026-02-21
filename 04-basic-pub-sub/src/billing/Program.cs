using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Billing;

class Program
{
    public static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        var endpoint = new BillingEndpoint(channel);
        await endpoint.StartAsync((message, correlationId) =>
        {
            Console.WriteLine($"Received {message} with correlation ID {correlationId}");
            Console.WriteLine($"Attempt to authorize customer credit card for order {correlationId}");
            return Task.CompletedTask;
        });

        Console.WriteLine(" Billing endpoint running.");
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}