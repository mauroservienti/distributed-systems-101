using System;
using System.Threading.Tasks;
using Billing.Events;
using NServiceBus;
using Sales.Events;

namespace Billing
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received OrderPlaced with order ID {message.OrderId}, correlation ID: {context.MessageHeaders[Headers.CorrelationId]}");

            var rnd = new Random(DateTime.Now.Millisecond);
            var delay = rnd.Next(1500, 3500);
            await Task.Delay(delay);

            await context.Publish(new PaymentAuthorized() {OrderId = message.OrderId});
            Console.WriteLine($"Published PaymentAuthorized, collecting items took {delay}ms.");
        }
    }
}