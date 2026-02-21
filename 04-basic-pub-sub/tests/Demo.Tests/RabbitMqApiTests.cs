using System;
using System.Threading.Tasks;
using Billing;
using RabbitMQ.Client;
using Sales;
using Testcontainers.RabbitMq;
using Website;
using Xunit;

namespace Demo.Tests;

public class PubSubTests(RabbitMqFixture fixture) : IClassFixture<RabbitMqFixture>
{
    /// <summary>
    /// Verifies the full Demo 04 flow: Website sends an order to Sales, Sales replies
    /// to Website and publishes an "order.accepted" event, and Billing receives that
    /// event — using a real RabbitMQ broker in Docker.
    /// </summary>
    [Fact]
    public async Task Website_sends_order_Sales_replies_and_Billing_receives_event()
    {
        var factory = new ConnectionFactory { Uri = new Uri(fixture.Container.GetConnectionString()) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var salesChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var billingChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var websiteChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        // Billing must subscribe to the exchange before Sales publishes to it
        var billingReceived = new TaskCompletionSource<(string message, string correlationId)>();
        var billingEndpoint = new BillingEndpoint(billingChannel);
        await billingEndpoint.StartAsync((message, correlationId) =>
        {
            billingReceived.TrySetResult((message, correlationId));
            return Task.CompletedTask;
        });

        // Sales receives orders, replies, and publishes events
        var salesEndpoint = new SalesEndpoint(salesChannel);
        await salesEndpoint.StartAsync();

        // Website listens for replies then sends an order
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

        // Billing receives the "order.accepted" event published by Sales
        var (billingMessage, billingCorrelationId) = await billingReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Contains("order-123", billingMessage);
        Assert.Equal("order-123", billingCorrelationId);
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
