using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Testing;
using Sales.Events;
using Warehouse;
using Warehouse.Events;
using Xunit;

namespace Demo.Tests;

public class WarehouseOrderPlacedHandlerTests
{
    [Fact]
    public async Task OrderPlaced_should_publish_OrderItemsCollected()
    {
        var handler = new OrderPlacedHandler();
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
