using System.Linq;
using System.Threading.Tasks;
using Billing.Events;
using NServiceBus.Testing;
using Sales;
using Sales.Events;
using Sales.Messages;
using Sales.Messages.Commands;
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

public class BillingOrderPlacedHandlerTests
{
    [Fact]
    public async Task OrderPlaced_should_publish_PaymentAuthorized()
    {
        var handler = new Billing.OrderPlacedHandler();
        var context = new TestableMessageHandlerContext();
        context.MessageHeaders[NServiceBus.Headers.CorrelationId] = "test-correlation-id";

        await handler.Handle(new OrderPlaced { OrderId = "order-1" }, context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<PaymentAuthorized>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal("order-1", published.OrderId);
    }
}

public class WarehouseOrderPlacedHandlerTests
{
    [Fact]
    public async Task OrderPlaced_should_publish_OrderItemsCollected()
    {
        var handler = new Warehouse.OrderPlacedHandler();
        var context = new TestableMessageHandlerContext();
        context.MessageHeaders[NServiceBus.Headers.CorrelationId] = "test-correlation-id";

        await handler.Handle(new OrderPlaced { OrderId = "order-1" }, context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<OrderItemsCollected>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal("order-1", published.OrderId);
    }
}
