namespace Modules.Budgeting.BackgroundJobs.Transactions.Sold;

public sealed class SellConfirmedJobData
{
    public required string? Ticker { get; init; }

    public required Guid TransactionId { get; init; }

    public required decimal TotalAmount { get; init; }

    public required int Quantity { get; init; }

    public required DateTime CreatedOnUtc { get; init; }

    public required string? UserEmail { get; init; }

    public required Guid UserId { get; init; }
}
