using System;
using System.Threading.Tasks;
using NServiceBus;

namespace Sales
{
    class Program
    {
        public static async Task Main()
        {
            var config = new EndpointConfiguration("sales");
            config.EnableInstallers();

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));
            conventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith("Messages"));

            config.UseTransport(
                new RabbitMQTransport(
                    RoutingTopology.Conventional(QueueType.Quorum), "host=localhost"));

            var endpoint = await Endpoint.Start(config);

            Console.WriteLine(" NServiceBus Sales endpoint running.");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            await endpoint.Stop();
        }
    }
}