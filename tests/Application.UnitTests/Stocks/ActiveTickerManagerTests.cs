using Modules.Stocks.Infrastructure.Realtime;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Stocks;

public sealed class ActiveTickerManagerTests
{
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(5);

    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ActiveTickerManager _manager;
    private DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public ActiveTickerManagerTests()
    {
        _clock.UtcNow.Returns(_ => _now);
        _manager = new ActiveTickerManager(_clock, Lease);
    }

    [Fact]
    public void Ticker_Should_StayActive_WhileAnyConnectionIsSubscribed()
    {
        _manager.Subscribe("c1", "aapl");
        _manager.Subscribe("c2", "AAPL");

        _manager.Unsubscribe("c1", "AAPL");

        Assert.Equal(["AAPL"], _manager.GetAllTickers());
    }

    [Fact]
    public void Ticker_Should_BecomeInactive_WhenLastSubscriberLeaves()
    {
        _manager.Subscribe("c1", "AAPL");
        _manager.Subscribe("c2", "AAPL");

        _manager.Unsubscribe("c1", "AAPL");
        _manager.Unsubscribe("c2", "AAPL");

        Assert.Empty(_manager.GetAllTickers());
    }

    [Fact]
    public void SubscribingTwice_FromSameConnection_Should_CountOnce()
    {
        _manager.Subscribe("c1", "AAPL");
        _manager.Subscribe("c1", "AAPL");

        _manager.Unsubscribe("c1", "AAPL");

        Assert.Empty(_manager.GetAllTickers());
    }

    [Fact]
    public void RemoveConnection_Should_ReleaseAllItsSubscriptions()
    {
        _manager.Subscribe("c1", "AAPL");
        _manager.Subscribe("c1", "MSFT");
        _manager.Subscribe("c2", "MSFT");

        _manager.RemoveConnection("c1");

        Assert.Equal(["MSFT"], _manager.GetAllTickers());
    }

    [Fact]
    public void RestLease_Should_ExpireWhenNobodySubscribes()
    {
        _manager.AddTicker("AAPL");
        Assert.Equal(["AAPL"], _manager.GetAllTickers());

        _now = _now.Add(Lease).AddSeconds(1);

        Assert.Empty(_manager.GetAllTickers());
    }

    [Fact]
    public void RestLease_Should_BeRenewedByLaterRequests()
    {
        _manager.AddTicker("AAPL");
        _now = _now.AddMinutes(4);
        _manager.AddTicker("AAPL");
        _now = _now.AddMinutes(4);

        Assert.Equal(["AAPL"], _manager.GetAllTickers());
    }

    [Fact]
    public void Subscription_Should_OutliveAnExpiredLease()
    {
        _manager.AddTicker("AAPL");
        _manager.Subscribe("c1", "AAPL");

        _now = _now.Add(Lease).AddMinutes(1);

        Assert.Equal(["AAPL"], _manager.GetAllTickers());
    }

    [Fact]
    public void AddTicker_Should_ReportWhetherTickerWasInactive()
    {
        Assert.True(_manager.AddTicker("AAPL"));
        Assert.False(_manager.AddTicker("aapl"));
    }
}
