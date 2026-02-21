using System;
using System.Threading.Tasks;
using Billing;
using RabbitMQ.Client;
using Sales;
using Testcontainers.RabbitMq;
using Warehouse;
using Website;
using Xunit;

namespace Demo.Tests;

public class MultipleSubscriberTests(RabbitMqFixture fixture) : IClassFixture<RabbitMqFixture>
{
    /// <summary>
    /// Verifies the full Demo 05 flow: Website sends an order to Sales, Sales replies
    /// to Website and publishes an "order.accepted" event, and BOTH Billing and Warehouse
    /// independently receive that event — using a real RabbitMQ broker in Docker.
    /// </summary>
    [Fact]
    public async Task Both_Billing_and_Warehouse_receive_the_order_accepted_event()
    {
        var factory = new ConnectionFactory { Uri = new Uri(fixture.Container.GetConnectionString()) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var salesChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var billingChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var warehouseChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var websiteChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        // Both subscribers must be set up before Sales publishes the event
        var billingReceived = new TaskCompletionSource<string>();
        var billingEndpoint = new BillingEndpoint(billingChannel);
        await billingEndpoint.StartAsync((message, _) =>
        {
            billingReceived.TrySetResult(message);
            return Task.CompletedTask;
        });

        var warehouseReceived = new TaskCompletionSource<string>();
        var warehouseEndpoint = new WarehouseEndpoint(warehouseChannel);
        await warehouseEndpoint.StartAsync((message, _) =>
        {
            warehouseReceived.TrySetResult(message);
            return Task.CompletedTask;
        });

        // Sales receives orders, replies, and publishes to all subscribers
        var salesEndpoint = new SalesEndpoint(salesChannel);
        await salesEndpoint.StartAsync();

        // Website sends an order
        var replyReceived = new TaskCompletionSource<string>();
        var websiteEndpoint = new WebsiteEndpoint(websiteChannel);
        await websiteEndpoint.StartAsync((message, _) =>
        {
            replyReceived.TrySetResult(message);
            return Task.CompletedTask;
        });

        await websiteEndpoint.SendOrderAsync("Hello World!", "order-123");

        // Website gets the Sales reply
        var reply = await replyReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Contains("order-123", reply);

        // Both Billing AND Warehouse receive the "order.accepted" event
        var billingMsg = await billingReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Contains("order-123", billingMsg);

        var warehouseMsg = await warehouseReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Contains("order-123", warehouseMsg);
    }
}

public class RabbitMqFixture : IAsyncLifetime
{
    public RabbitMqContainer Container { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Container = new RabbitMqBuilder("rabbitmq:4-management-alpine").Build();
        await Container.StartAsync();
    }

    public async Task DisposeAsync() => await Container.DisposeAsync();
}
