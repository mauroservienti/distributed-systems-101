using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Sales;
using Testcontainers.RabbitMq;
using Website;
using Xunit;

namespace Demo.Tests;

public class RequestResponseTests(RabbitMqFixture fixture) : IClassFixture<RabbitMqFixture>
{
    /// <summary>
    /// Verifies the full Demo 02 flow: Website sends a request to the "sales" queue
    /// with a ReplyTo address, and Sales processes it and sends a reply back —
    /// using a real RabbitMQ broker in Docker.
    /// </summary>
    [Fact]
    public async Task Website_sends_order_and_receives_reply_from_Sales()
    {
        var factory = new ConnectionFactory { Uri = new Uri(fixture.Container.GetConnectionString()) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var salesChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var websiteChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        // Sales listens for orders and automatically sends a reply
        var salesEndpoint = new SalesEndpoint(salesChannel);
        await salesEndpoint.StartAsync();

        // Website listens for replies
        var replyReceived = new TaskCompletionSource<(string message, string correlationId)>();
        var websiteEndpoint = new WebsiteEndpoint(websiteChannel);
        await websiteEndpoint.StartAsync((message, correlationId) =>
        {
            replyReceived.TrySetResult((message, correlationId));
            return Task.CompletedTask;
        });

        // Website sends an order request
        await websiteEndpoint.SendOrderAsync("Hello World!", "order-123");

        // Assert the reply was received with the correct correlation ID
        var (replyMessage, replyCorrelationId) = await replyReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Contains("order-123", replyMessage);
        Assert.Equal("order-123", replyCorrelationId);
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
