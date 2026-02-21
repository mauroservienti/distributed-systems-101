using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Sales;
using Testcontainers.RabbitMq;
using Website;
using Xunit;

namespace Demo.Tests;

public class BasicSendTests(RabbitMqFixture fixture) : IClassFixture<RabbitMqFixture>
{
    /// <summary>
    /// Verifies the full Demo 01 flow: Website sends a plain message to the "sales"
    /// queue and Sales receives it — using a real RabbitMQ broker in Docker.
    /// </summary>
    [Fact]
    public async Task Website_sends_message_and_Sales_receives_it()
    {
        var factory = new ConnectionFactory { Uri = new Uri(fixture.Container.GetConnectionString()) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var receiverChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var senderChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        var received = new TaskCompletionSource<string>();
        var salesEndpoint = new SalesEndpoint(receiverChannel);
        await salesEndpoint.StartAsync(msg =>
        {
            received.TrySetResult(msg);
            return Task.CompletedTask;
        });

        var websiteEndpoint = new WebsiteEndpoint(senderChannel);
        await websiteEndpoint.SendOrderAsync("Hello World!");

        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("Hello World!", message);
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
