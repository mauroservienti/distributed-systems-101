using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Website
{
    class Program
    {
        public static async Task Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

            var endpoint = new WebsiteEndpoint(channel);

            Console.WriteLine(" Press [enter] to send a message.");
            Console.ReadLine();

            const string message = "Hello World!";
            await endpoint.SendOrderAsync(message);
            Console.WriteLine(" [x] Sent {0}", message);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
