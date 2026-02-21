using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Testing;
using Sales.Messages.Events;
using Sales.Service.Policies;
using Xunit;

namespace Demo.Tests;

file class TestProductAddedToCart : ProductAddedToCart
{
    public Guid CartId { get; set; }
    public int ProductId { get; set; }
}

public class ShoppingCartLifecyclePolicyTests
{
    [Fact]
    public async Task ProductAddedToCart_should_update_last_touched_and_request_two_timeouts()
    {
        var saga = new ShoppingCartLifecyclePolicy
        {
            Data = new ShoppingCartLifecyclePolicy.ShoppingCartLifecyclePolicyData()
        };
        var context = new TestableMessageHandlerContext();
        var cartId = Guid.NewGuid();

        await saga.Handle(new TestProductAddedToCart { CartId = cartId, ProductId = 1 }, context);

        Assert.Equal(2, context.TimeoutMessages.Length);
        Assert.True(saga.Data.LastTouched > DateTime.MinValue);
    }

    [Fact]
    public async Task StaleTimeout_when_cart_untouched_since_scheduled_should_publish_ShoppingCartGotStale()
    {
        var cartId = Guid.NewGuid();
        var lastTouched = DateTime.Now.AddSeconds(-60);
        var saga = new ShoppingCartLifecyclePolicy
        {
            Data = new ShoppingCartLifecyclePolicy.ShoppingCartLifecyclePolicyData
            {
                CartId = cartId,
                LastTouched = lastTouched
            }
        };
        var context = new TestableMessageHandlerContext();

        await saga.Timeout(new CartGettingStaleTimeout { LastTouched = lastTouched }, context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<ShoppingCartGotStale>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal(cartId, published.CartId);
    }

    [Fact]
    public async Task StaleTimeout_when_cart_was_touched_after_scheduled_should_not_publish()
    {
        var cartId = Guid.NewGuid();
        var timeoutLastTouched = DateTime.Now.AddSeconds(-60);
        var newerLastTouched = DateTime.Now.AddSeconds(-10); // cart touched after timeout scheduled
        var saga = new ShoppingCartLifecyclePolicy
        {
            Data = new ShoppingCartLifecyclePolicy.ShoppingCartLifecyclePolicyData
            {
                CartId = cartId,
                LastTouched = newerLastTouched
            }
        };
        var context = new TestableMessageHandlerContext();

        await saga.Timeout(new CartGettingStaleTimeout { LastTouched = timeoutLastTouched }, context);

        Assert.Empty(context.PublishedMessages);
    }

    [Fact]
    public async Task WipeTimeout_when_cart_untouched_should_publish_ShoppingCartGotInactive_and_complete()
    {
        var cartId = Guid.NewGuid();
        var lastTouched = DateTime.Now.AddSeconds(-120);
        var saga = new ShoppingCartLifecyclePolicy
        {
            Data = new ShoppingCartLifecyclePolicy.ShoppingCartLifecyclePolicyData
            {
                CartId = cartId,
                LastTouched = lastTouched
            }
        };
        var context = new TestableMessageHandlerContext();

        await saga.Timeout(new CartWipeTimeout { LastTouched = lastTouched }, context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<ShoppingCartGotInactive>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal(cartId, published.CartId);
        Assert.True(saga.Completed);
    }
}
