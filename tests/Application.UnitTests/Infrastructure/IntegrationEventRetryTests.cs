using System.Text;
using Infrastructure.Events;

namespace Application.UnitTests.Infrastructure;

public sealed class IntegrationEventRetryTests
{
    [Fact]
    public void GetAttempt_Should_BeOne_ForAFirstDelivery()
    {
        Assert.Equal(1, IntegrationEventProcessorJob.GetAttempt(null));
        Assert.Equal(1, IntegrationEventProcessorJob.GetAttempt(new Dictionary<string, object?>()));
    }

    [Fact]
    public void GetAttempt_Should_ReadTheHeaderInTheShapesRabbitMqReturns()
    {
        const string header = IntegrationEventProcessorJob.AttemptHeader;

        Assert.Equal(3, IntegrationEventProcessorJob.GetAttempt(new Dictionary<string, object?> { [header] = 3 }));
        Assert.Equal(4, IntegrationEventProcessorJob.GetAttempt(new Dictionary<string, object?> { [header] = 4L }));
        Assert.Equal(2, IntegrationEventProcessorJob.GetAttempt(
            new Dictionary<string, object?> { [header] = Encoding.UTF8.GetBytes("2") }));
    }

    [Fact]
    public void ShouldDiscard_Should_StopAfterMaxAttempts()
    {
        Assert.False(IntegrationEventProcessorJob.ShouldDiscard(IntegrationEventProcessorJob.MaxAttempts - 1));
        Assert.True(IntegrationEventProcessorJob.ShouldDiscard(IntegrationEventProcessorJob.MaxAttempts));
    }
}
