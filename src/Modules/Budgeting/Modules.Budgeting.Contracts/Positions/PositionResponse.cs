namespace Modules.Budgeting.Contracts.Positions;

/// <summary>
/// A current holding. <see cref="AverageCost"/> is the average price paid per share still held
/// (average-cost method) and <see cref="TotalCost"/> is the cost basis of those shares, in USD.
/// </summary>
public sealed record PositionResponse(string Ticker, int Quantity, decimal AverageCost, decimal TotalCost);
