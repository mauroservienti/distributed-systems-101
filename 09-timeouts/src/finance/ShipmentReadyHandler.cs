using System;
using System.Threading.Tasks;
using NServiceBus;
using Shipping.Events;

namespace Finance
{
    public class ShipmentReadyHandler : IHandleMessages<ShipmentReady>
    {
        public Task Handle(ShipmentReady message, IMessageHandlerContext context)
        {
            return context.Publish(new Events.InvoiceIssued()
            {
                OrderId = message.OrderId,
                InvoiceNumber = (int)DateTime.Now.Ticks,
                CustomerCountry = "Italy",
                DueDate = DateTime.Now.AddDays(15)
            });
        }
    }
}