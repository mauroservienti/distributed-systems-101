using System;
using System.Threading.Tasks;
using NServiceBus;
using Sales.Events;
using Sales.Messages;
using Sales.Messages.Commands;

namespace Sales
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received PlaceOrder with order ID {message.OrderId}, correlation ID: {context.MessageHeaders[Headers.CorrelationId]}");

            await context.Reply(new PlaceOrderReply() {OrderId = message.OrderId});
            Console.WriteLine($"Replied OrderReceived to {context.ReplyToAddress}");

            await context.Publish(new OrderPlaced() {OrderId = message.OrderId});
            Console.WriteLine("Published OrderPlaced");
        }
    }
}