namespace Modules.Budgeting.Api.Responses;

/// <param name="AverageCost">Average price paid per share still held (average-cost method).</param>
/// <param name="TotalCost">Cost basis of the shares still held.</param>
public sealed record PositionApiResponse(string Ticker, int Quantity, decimal AverageCost, decimal TotalCost);
