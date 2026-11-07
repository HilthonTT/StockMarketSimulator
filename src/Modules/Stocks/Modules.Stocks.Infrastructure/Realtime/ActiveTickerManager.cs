using Modules.Stocks.Application.Abstractions.Realtime;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Realtime;

/// <summary>
/// In-memory, reference-counted set of active tickers. See <see cref="IActiveTickerManager"/>.
/// </summary>
internal sealed class ActiveTickerManager : IActiveTickerManager
{
    /// <summary>How long a ticker quoted over REST stays active without any subscriber.</summary>
    public static readonly TimeSpan DefaultRestLease = TimeSpan.FromMinutes(5);

    private readonly Lock _lock = new();
    private readonly Dictionary<string, HashSet<string>> _subscribersByTicker = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> _tickersByConnection = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTime> _restLeases = new(StringComparer.Ordinal);

    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly TimeSpan _restLease;

    public ActiveTickerManager(IDateTimeProvider dateTimeProvider)
        : this(dateTimeProvider, DefaultRestLease)
    {
    }

    internal ActiveTickerManager(IDateTimeProvider dateTimeProvider, TimeSpan restLease)
    {
        _dateTimeProvider = dateTimeProvider;
        _restLease = restLease;
    }

    public bool AddTicker(string ticker)
    {
        ticker = Normalize(ticker);
        DateTime now = _dateTimeProvider.UtcNow;

        lock (_lock)
        {
            bool wasActive = IsActive(ticker, now);

            _restLeases[ticker] = now.Add(_restLease);

            return !wasActive;
        }
    }

    public void Subscribe(string connectionId, string ticker)
    {
        ticker = Normalize(ticker);

        lock (_lock)
        {
            if (!_subscribersByTicker.TryGetValue(ticker, out HashSet<string>? subscribers))
            {
                subscribers = new HashSet<string>(StringComparer.Ordinal);
                _subscribersByTicker[ticker] = subscribers;
            }

            subscribers.Add(connectionId);

            if (!_tickersByConnection.TryGetValue(connectionId, out HashSet<string>? tickers))
            {
                tickers = new HashSet<string>(StringComparer.Ordinal);
                _tickersByConnection[connectionId] = tickers;
            }

            tickers.Add(ticker);
        }
    }

    public void Unsubscribe(string connectionId, string ticker)
    {
        ticker = Normalize(ticker);

        lock (_lock)
        {
            RemoveSubscription(connectionId, ticker);

            if (_tickersByConnection.TryGetValue(connectionId, out HashSet<string>? tickers))
            {
                tickers.Remove(ticker);

                if (tickers.Count == 0)
                {
                    _tickersByConnection.Remove(connectionId);
                }
            }
        }
    }

    public void RemoveConnection(string connectionId)
    {
        lock (_lock)
        {
            if (!_tickersByConnection.Remove(connectionId, out HashSet<string>? tickers))
            {
                return;
            }

            foreach (string ticker in tickers)
            {
                RemoveSubscription(connectionId, ticker);
            }
        }
    }

    public IReadOnlyCollection<string> GetAllTickers()
    {
        DateTime now = _dateTimeProvider.UtcNow;

        lock (_lock)
        {
            // Expired leases are pruned here, so the dictionary stays bounded by recent traffic.
            List<string> expired = [.. _restLeases.Where(lease => lease.Value <= now).Select(lease => lease.Key)];
            foreach (string ticker in expired)
            {
                _restLeases.Remove(ticker);
            }

            return _subscribersByTicker.Keys
                .Union(_restLeases.Keys, StringComparer.Ordinal)
                .ToList()
                .AsReadOnly();
        }
    }

    private void RemoveSubscription(string connectionId, string ticker)
    {
        if (_subscribersByTicker.TryGetValue(ticker, out HashSet<string>? subscribers))
        {
            subscribers.Remove(connectionId);

            if (subscribers.Count == 0)
            {
                _subscribersByTicker.Remove(ticker);
            }
        }
    }

    private bool IsActive(string ticker, DateTime now) =>
        _subscribersByTicker.ContainsKey(ticker) ||
        (_restLeases.TryGetValue(ticker, out DateTime expiresOn) && expiresOn > now);

    private static string Normalize(string ticker) => ticker.Trim().ToUpperInvariant();
}
