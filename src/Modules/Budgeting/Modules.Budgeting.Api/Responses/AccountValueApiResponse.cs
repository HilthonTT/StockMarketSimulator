namespace Modules.Budgeting.Api.Responses;

/// <summary>A user's account valued at current prices.</summary>
public sealed record AccountValueApiResponse(
    Guid UserId,
    decimal Cash,
    decimal HoldingsValue,
    decimal AccountValue,
    decimal ReturnPercent,
    int Positions);
