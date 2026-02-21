using System.Linq;
using System.Threading.Tasks;
using Billing;
using Billing.Events;
using NServiceBus.Testing;
using Sales.Events;
using Xunit;

namespace Demo.Tests;

public class BillingOrderPlacedHandlerTests
{
    [Fact]
    public async Task OrderPlaced_should_publish_PaymentAuthorized()
    {
        var handler = new OrderPlacedHandler();
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
