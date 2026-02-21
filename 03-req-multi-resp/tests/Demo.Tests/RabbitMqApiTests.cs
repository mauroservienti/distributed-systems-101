using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Sales;
using Testcontainers.RabbitMq;
using Website;
using Xunit;

namespace Demo.Tests;

public class MultipleResponseTests(RabbitMqFixture fixture) : IClassFixture<RabbitMqFixture>
{
    /// <summary>
    /// Verifies the full Demo 03 flow: Website sends a request to the "sales" queue and
    /// Sales sends back TWO replies — an initial acknowledgement and a later "shipped"
    /// update — using a real RabbitMQ broker in Docker.
    /// </summary>
    [Fact]
    public async Task Website_sends_order_and_receives_two_replies_from_Sales()
    {
        var factory = new ConnectionFactory { Uri = new Uri(fixture.Container.GetConnectionString()) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var salesChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await using var websiteChannel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));

        // Sales sends two replies; pass TimeSpan.Zero so the test is not slow
        var salesEndpoint = new SalesEndpoint(salesChannel, TimeSpan.Zero);
        await salesEndpoint.StartAsync();

        // Website collects all replies
        var replies = new List<string>();
        var secondReply = new TaskCompletionSource<bool>();
        var websiteEndpoint = new WebsiteEndpoint(websiteChannel);
        await websiteEndpoint.StartAsync((message, _) =>
        {
            lock (replies) replies.Add(message);
            if (replies.Count >= 2) secondReply.TrySetResult(true);
            return Task.CompletedTask;
        });

        await websiteEndpoint.SendOrderAsync("Hello World!", "order-123");

        await secondReply.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(2, replies.Count);
        Assert.Contains(replies, r => r.Contains("on its way"));
        Assert.Contains(replies, r => r.Contains("shipped"));
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
