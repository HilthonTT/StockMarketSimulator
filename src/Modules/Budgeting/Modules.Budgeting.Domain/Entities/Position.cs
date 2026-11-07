using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Domain.Entities;

/// <summary>
/// A user's current holding of one ticker, maintained in the same transaction as every buy and sell.
/// </summary>
/// <remarks>
/// Cost basis uses the average-cost method: a buy adds <c>price x quantity</c> to <see cref="TotalCost"/>,
/// a sell removes the sold shares at the current average cost, so selling never changes
/// <see cref="AverageCost"/>. A position that drops to zero shares is kept with a zero cost basis.
/// </remarks>
public sealed class Position : Entity, IAuditable
{
    /// <summary>Decimal places kept for the cost basis.</summary>
    public const int CostDecimals = 4;

    private Position(Guid id, Guid userId, string ticker)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));

        Id = id;
        UserId = userId;
        Ticker = ticker;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Position"/>
    /// </summary>
    /// <remarks>
    /// Required for EF Core
    /// </remarks>
    private Position()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Ticker { get; private set; } = null!;

    public int Quantity { get; private set; }

    /// <summary>The cost basis of the shares currently held.</summary>
    public decimal TotalCost { get; private set; }

    public decimal AverageCost => Quantity == 0 ? 0m : Math.Round(TotalCost / Quantity, CostDecimals);

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static Position Open(Guid userId, string ticker)
    {
        return new Position(Guid.CreateVersion7(), userId, NormalizeTicker(ticker));
    }

    public static string NormalizeTicker(string ticker)
    {
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));

        return ticker.Trim().ToUpperInvariant();
    }

    public Result Buy(int quantity, decimal pricePerShare)
    {
        if (quantity <= 0)
        {
            return Result.Failure(TransactionErrors.NegativeQuantityNotAllowed);
        }

        if (pricePerShare < 0)
        {
            return Result.Failure(BudgetErrors.NegativeAmountNotAllowed);
        }

        Quantity += quantity;
        TotalCost = Math.Round(TotalCost + (pricePerShare * quantity), CostDecimals);

        return Result.Success();
    }

    public Result Sell(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(TransactionErrors.NegativeQuantityNotAllowed);
        }

        if (quantity > Quantity)
        {
            return Result.Failure(TransactionErrors.InsufficientStock);
        }

        int remaining = Quantity - quantity;

        TotalCost = remaining == 0
            ? 0m
            : Math.Round(TotalCost * remaining / Quantity, CostDecimals);

        Quantity = remaining;

        return Result.Success();
    }
}
