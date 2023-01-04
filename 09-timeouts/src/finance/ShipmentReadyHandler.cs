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
                CustomerCountry = "Wakanda",
                // We don't want to wait to much for the invoice to be due
                DueDate = DateTime.Now.AddSeconds(90)
            });
        }
    }
}