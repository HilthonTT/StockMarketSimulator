namespace Modules.Stocks.Application.Abstractions.Realtime;

/// <summary>
/// Tracks which tickers the real-time feed has to simulate.
/// </summary>
/// <remarks>
/// A ticker is active while at least one SignalR connection is subscribed to it (reference-counted per
/// connection), or for a short lease after it was last quoted over REST. When neither holds, it drops
/// out of the active set and the feed stops updating it.
/// </remarks>
public interface IActiveTickerManager
{
    /// <summary>
    /// Marks a ticker as recently requested (REST quote). It stays active for the lease period.
    /// </summary>
    /// <returns><c>true</c> if the ticker was not active before.</returns>
    bool AddTicker(string ticker);

    /// <summary>Records that a connection subscribed to a ticker. Subscribing twice counts once.</summary>
    void Subscribe(string connectionId, string ticker);

    /// <summary>Records that a connection unsubscribed from a ticker.</summary>
    void Unsubscribe(string connectionId, string ticker);

    /// <summary>Drops every subscription held by a connection (e.g. on disconnect).</summary>
    void RemoveConnection(string connectionId);

    /// <summary>Gets the tickers that currently have subscribers or an unexpired REST lease.</summary>
    IReadOnlyCollection<string> GetAllTickers();
}
