using System;
using System.Threading.Tasks;
using NServiceBus;
using Sales.Events;
using Warehouse.Events;

namespace Warehouse
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received OrderPlaced with order ID {message.OrderId}, correlation ID: {context.MessageHeaders[Headers.CorrelationId]}");

            var rnd = new Random(DateTime.Now.Millisecond);
            var delay = rnd.Next(1000, 3000);
            await Task.Delay(delay);

            await context.Publish(new OrderItemsCollected() {OrderId = message.OrderId});
            Console.WriteLine($"Published OrderItemsCollected, collecting items took {delay}ms.");
        }
    }
}