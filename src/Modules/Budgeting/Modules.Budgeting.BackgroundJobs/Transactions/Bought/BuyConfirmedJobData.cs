namespace Modules.Budgeting.BackgroundJobs.Transactions.Bought;

public sealed class BuyConfirmedJobData
{
    public Guid TransactionId { get; init; }

    public decimal TotalAmount { get; init; }

    public int Quantity { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public string? UserEmail { get; init; } = string.Empty;

    public Guid UserId { get; init; }
}
