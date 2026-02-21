using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace Demo.Tests;

/// <summary>
/// Smoke tests that verify the RabbitMQ.Client API surface used in the demo still compiles
/// and the key types can be instantiated. These tests catch breaking API changes in the
/// RabbitMQ.Client package without requiring a running broker.
/// </summary>
public class RabbitMqApiTests
{
    [Fact]
    public void ConnectionFactory_can_be_created()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        Assert.NotNull(factory);
    }

    [Fact]
    public void CreateChannelOptions_can_be_created()
    {
        var options = new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true);
        Assert.NotNull(options);
    }

    [Fact]
    public void BasicProperties_can_be_created_with_correlation_id()
    {
        var props = new BasicProperties { CorrelationId = "order-abc" };
        Assert.Equal("order-abc", props.CorrelationId);
    }
}
