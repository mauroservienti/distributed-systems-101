using System;
using System.Threading.Tasks;
using Billing.Events;
using NServiceBus;
using Shipping.Events;
using Warehouse.Events;

namespace Shipping
{
    public class ShippingPolicy : Saga<ShippingPolicyData>,
        IAmStartedByMessages<PaymentAuthorized>,
        IAmStartedByMessages<OrderItemsCollected>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
        {
            mapper.MapSaga(d => d.OrderId)
                .ToMessage<PaymentAuthorized>(m => m.OrderId)
                .ToMessage<OrderItemsCollected>(m => m.OrderId);
        }

        public Task Handle(PaymentAuthorized message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Handling {typeof(PaymentAuthorized)} for order {message.OrderId}.");
            Data.IsPaymentAuthorized = true;
            return VerifyStatus(message.OrderId, context);
        }

        public Task Handle(OrderItemsCollected message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Handling {typeof(OrderItemsCollected)} for order {message.OrderId}.");
            Data.AreOrderItemsCollected = true;
            return VerifyStatus(message.OrderId, context);
        }
        
        private Task VerifyStatus(string orderId, IMessageHandlerContext context)
        {
            if (Data.IsPaymentAuthorized && Data.AreOrderItemsCollected)
            {
                Console.WriteLine($"Order {orderId} is ready to be shipped.");
                //We wanna see data in the db during the demos
                //MarkAsComplete();
                return context.Publish(new ShipmentReady() {OrderId = Data.OrderId});
            }

            return Task.CompletedTask;
        }
    }

    public class ShippingPolicyData : ContainSagaData
    {
        public string OrderId { get; set; }
        public bool IsPaymentAuthorized { get; set; }
        public bool AreOrderItemsCollected { get; set; }
    }
}