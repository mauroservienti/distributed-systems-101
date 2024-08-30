﻿using System;
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
            config.UseSerialization<SystemJsonSerializer>();

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));
            conventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith("Messages"));

            var transport = config.UseTransport(
                new RabbitMQTransport(
                    RoutingTopology.Conventional(QueueType.Classic),
                    "host=localhost"
                    )
                );

            transport.RouteToEndpoint(typeof(PlaceOrder).Assembly, "sales");

            var endpoint = await Endpoint.Start(config);

            Console.WriteLine(" NServiceBus Website endpoint running.");
            Console.WriteLine(" Press [enter] to send a message.");
            Console.ReadLine();

            var message = new PlaceOrder() {OrderId = Guid.NewGuid().ToString() };
            await endpoint.Send(message);

            Console.WriteLine(" Message sent. Press [enter] to exit.");
            Console.ReadLine();

            await endpoint.Stop();
        }
    }
}