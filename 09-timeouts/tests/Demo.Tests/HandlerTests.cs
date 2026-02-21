using System;
using System.Linq;
using System.Threading.Tasks;
using Billing.Events;
using Finance;
using Finance.Events;
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

public class ShipmentReadyHandlerTests
{
    [Fact]
    public async Task ShipmentReady_should_publish_InvoiceIssued()
    {
        var handler = new ShipmentReadyHandler();
        var context = new TestableMessageHandlerContext();

        await handler.Handle(new ShipmentReady { OrderId = "order-1" }, context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<InvoiceIssued>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal("order-1", published.OrderId);
    }
}

public class OverdueInvoicePolicyTests
{
    [Fact]
    public async Task InvoiceIssued_should_store_data_and_request_timeout()
    {
        var saga = new OverdueInvoicePolicy { Data = new OverdueInvoiceData() };
        var context = new TestableMessageHandlerContext();
        var dueDate = DateTime.UtcNow.AddDays(30);

        await saga.Handle(new InvoiceIssued
        {
            InvoiceNumber = 42,
            OrderId = "order-1",
            CustomerCountry = "USA",
            DueDate = dueDate
        }, context);

        Assert.Equal(42, saga.Data.InvoiceNumber);
        Assert.Equal("order-1", saga.Data.OrderId);
        Assert.Single(context.TimeoutMessages);
        Assert.IsType<CheckPayment>(context.TimeoutMessages[0].Message);
    }

    [Fact]
    public async Task InvoiceIssued_for_Italy_should_extend_due_date_by_20_days()
    {
        var saga = new OverdueInvoicePolicy { Data = new OverdueInvoiceData() };
        var context = new TestableMessageHandlerContext();
        var dueDate = DateTime.UtcNow.AddDays(30);

        await saga.Handle(new InvoiceIssued
        {
            InvoiceNumber = 43,
            OrderId = "order-2",
            CustomerCountry = "Italy",
            DueDate = dueDate
        }, context);

        Assert.Single(context.TimeoutMessages);
        var scheduledAt = context.TimeoutMessages[0].At;
        Assert.NotNull(scheduledAt);
        Assert.True(scheduledAt.Value > dueDate.AddDays(19), "Italy invoices should have at least 20 extra days");
    }

    [Fact]
    public async Task InvoicePaid_should_mark_saga_complete()
    {
        var saga = new OverdueInvoicePolicy { Data = new OverdueInvoiceData { InvoiceNumber = 42 } };
        var context = new TestableMessageHandlerContext();

        await saga.Handle(new InvoicePaid { InvoiceNumber = 42 }, context);

        Assert.True(saga.Completed);
    }

    [Fact]
    public async Task Timeout_should_publish_InvoiceOverdue_and_complete()
    {
        var saga = new OverdueInvoicePolicy { Data = new OverdueInvoiceData { InvoiceNumber = 42 } };
        var context = new TestableMessageHandlerContext();

        await saga.Timeout(new CheckPayment(), context);

        var published = context.PublishedMessages
            .Select(m => m.Message)
            .OfType<InvoiceOverdue>()
            .SingleOrDefault();
        Assert.NotNull(published);
        Assert.Equal(42, published.InvoiceNumber);
        Assert.True(saga.Completed);
    }
}
