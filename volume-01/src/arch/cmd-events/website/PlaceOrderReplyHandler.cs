using System;
using System.Threading.Tasks;
using NServiceBus;
using Sales.Messages;

namespace Website
{
    public class PlaceOrderReplyHandler : IHandleMessages<PlaceOrderReply>
    {
        public Task Handle(PlaceOrderReply message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received PlaceOrderReply with order ID {message.OrderId}, correlation ID: {context.MessageHeaders[Headers.CorrelationId]}");
            //Do something meaningful
            return Task.CompletedTask;
        }
    }
}