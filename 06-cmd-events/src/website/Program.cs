using System;
using System.Threading.Tasks;
using NServiceBus;
using Sales.Messages.Commands;

namespace Website
{
    class Program
    {
        public static async Task Main()
        {
            var config = new EndpointConfiguration("website");
            config.EnableInstallers();

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));
            conventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith("Messages"));

            var transport = config.UseTransport(
                new RabbitMQTransport(
                    RoutingTopology.Conventional(QueueType.Classic), "host=localhost"));

            transport.RouteToEndpoint(typeof(PlaceOrder).Assembly, "sales");

            var endpoint = await Endpoint.Start(config);

            var message = new PlaceOrder() {OrderId = Guid.NewGuid().ToString() };
            await endpoint.Send(message);

            Console.WriteLine(" NServiceBus Website endpoint running.");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            await endpoint.Stop();
        }
    }
}