using System.Linq;
using System.Threading.Tasks;
using Billing.Events;
using NServiceBus.Testing;
using Sales;
using Sales.Events;
using Sales.Messages;
using Sales.Messages.Commands;
using Shipping;
using Shipping.Events;
using Warehouse.Events;
using Xunit;

namespace Demo.Tests;

public class PlaceOrderHandlerTests
{
    [Fact]
    public async Task PlaceOrder_should_reply_with_PlaceOrderReply()
    {
        var handler = new PlaceOrderHandler();
        var context = new TestableMessageHandlerContext();
        context.MessageHeaders[NServiceBus.Headers.CorrelationId] = "test-correlation-id";

        await handler.Handle(new PlaceOrder { OrderId = "order-1" }, context);

        var reply = context.RepliedMessages
            .Select(m => m.Message)
            .OfType<PlaceOrderReply>()
            .SingleOrDefault();
        Assert.NotNull(reply);
        Assert.Equal("order-1", reply.OrderId);
    }

    [Fact]
    public async Task PlaceOrder_should_publish_OrderPlaced()
    {
        var handler = new PlaceOrderHandler();
        var context = new TestableMessageHandlerContext();
        context.MessageHeaders[NServiceBus.Headers.CorrelationId] = "test-correlation-id";

        await handler.Handle(new PlaceOrder { OrderId = "order-1" }, context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<OrderPlaced>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal("order-1", published.OrderId);
    }
}

public class ShippingPolicyTests
{
    [Fact]
    public async Task PaymentAuthorized_alone_should_not_publish_ShipmentReady()
    {
        var saga = new ShippingPolicy { Data = new ShippingPolicyData() };
        var context = new TestableMessageHandlerContext();

        await saga.Handle(new PaymentAuthorized { OrderId = "order-1" }, context);

        Assert.Empty(context.PublishedMessages);
        Assert.True(saga.Data.IsPaymentAuthorized);
        Assert.False(saga.Data.AreOrderItemsCollected);
    }

    [Fact]
    public async Task OrderItemsCollected_alone_should_not_publish_ShipmentReady()
    {
        var saga = new ShippingPolicy { Data = new ShippingPolicyData() };
        var context = new TestableMessageHandlerContext();

        await saga.Handle(new OrderItemsCollected { OrderId = "order-1" }, context);

        Assert.Empty(context.PublishedMessages);
        Assert.False(saga.Data.IsPaymentAuthorized);
        Assert.True(saga.Data.AreOrderItemsCollected);
    }

    [Fact]
    public async Task Both_messages_received_should_publish_ShipmentReady()
    {
        var saga = new ShippingPolicy { Data = new ShippingPolicyData { OrderId = "order-1" } };
        var context = new TestableMessageHandlerContext();

        await saga.Handle(new PaymentAuthorized { OrderId = "order-1" }, context);
        await saga.Handle(new OrderItemsCollected { OrderId = "order-1" }, context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<ShipmentReady>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal("order-1", published.OrderId);
    }
}
