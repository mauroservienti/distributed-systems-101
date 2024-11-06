using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Sales
{
    class Program
    {
        public static async Task Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
            
            await channel.QueueDeclareAsync(queue: "sales",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
                
                return Task.CompletedTask;
            };
            await channel.BasicConsumeAsync(queue: "sales",
                autoAck: true,
                consumer: consumer);

            Console.WriteLine(" Sales endpoint running.");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
