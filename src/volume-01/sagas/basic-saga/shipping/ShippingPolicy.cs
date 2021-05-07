using System.Threading.Tasks;
using Billing.Events;
using NServiceBus;
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
            throw new System.NotImplementedException();
        }

        public Task Handle(OrderItemsCollected message, IMessageHandlerContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ShippingPolicyData : ContainSagaData
    {
        public string OrderId { get; set; }
        public bool IsPaymentAuthorized { get; set; }
        public bool AreOrderItemsCollected { get; set; }
    }
}