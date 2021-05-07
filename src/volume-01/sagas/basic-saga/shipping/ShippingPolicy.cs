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
            Data.IsPaymentAuthorized = true;
            return VerifyStatus(context);
        }

        public Task Handle(OrderItemsCollected message, IMessageHandlerContext context)
        {
            Data.AreOrderItemsCollected = true;
            return VerifyStatus(context);
        }
        
        private Task VerifyStatus(IMessageHandlerContext context)
        {
            if (Data.IsPaymentAuthorized && Data.AreOrderItemsCollected)
            {
                MarkAsComplete();
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