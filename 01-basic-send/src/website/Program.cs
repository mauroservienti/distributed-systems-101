using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

namespace Website
{
    class Program
    {
        public static async Task Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

            Console.WriteLine(" Press [enter] to send a message.");
            Console.ReadLine();

            const string message = "Hello World!";
            await channel.BasicPublishAsync(
                "", 
                "sales", 
                true, 
                Encoding.UTF8.GetBytes(message));
            
            Console.WriteLine(" [x] Sent {0}", message);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
