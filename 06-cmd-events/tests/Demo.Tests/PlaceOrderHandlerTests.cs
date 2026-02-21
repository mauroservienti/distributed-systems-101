using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Testing;
using Sales;
using Sales.Messages;
using Sales.Messages.Commands;
using Sales.Events;
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
