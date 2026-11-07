using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Api.Api;
using Modules.Budgeting.Api.Responses;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Errors;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.GetTopPerfomer;

internal sealed class GetTopPerformerQueryHandler(
    IUserContext userContext,
    IStockService stockService,
    IBudgetingApi budgetingApi) : IQueryHandler<GetTopPerformerQuery, StockPriceResponse>
{
    public async Task<Result<StockPriceResponse>> Handle(
        GetTopPerformerQuery request, 
        CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<StockPriceResponse>(UserErrors.Unauthorized);
        }

        List<TransactionApiResponse> transactions = await budgetingApi.GetTransactionsByUserIdAsync(
            userContext.UserId,
            cancellationToken);

        const int BuyType = 1; // TransactionType.Expense

        // Cost basis per open position: the quantity-weighted average buy price.
        // Positions that have been fully sold are not "performers" any more.
        var openPositions = transactions
            .GroupBy(t => t.Ticker)
            .Select(g =>
            {
                List<TransactionApiResponse> buys = [.. g.Where(t => t.Type == BuyType)];
                int boughtQuantity = buys.Sum(t => t.Quantity);
                int netQuantity = boughtQuantity - g.Where(t => t.Type != BuyType).Sum(t => t.Quantity);

                decimal averageCost = boughtQuantity == 0
                    ? 0
                    : buys.Sum(t => t.Amount * t.Quantity) / boughtQuantity;

                return (Ticker: g.Key, NetQuantity: netQuantity, AverageCost: averageCost);
            })
            .Where(p => p.NetQuantity > 0 && p.AverageCost > 0)
            .ToList();

        List<(StockPriceResponse Stock, decimal Performance)> performances = [];

        foreach ((string ticker, _, decimal averageCost) in openPositions)
        {
            Option<StockPriceResponse> stockOption = await stockService.GetLatestStockPriceAsync(
                ticker,
                cancellationToken);

            if (!stockOption.IsSome)
            {
                continue;
            }

            StockPriceResponse stock = stockOption.ValueOrThrow();

            decimal performance = (stock.Price - averageCost) / averageCost;

            performances.Add((stock, performance));
        }

        StockPriceResponse? topPerformer = performances
           .OrderByDescending(p => p.Performance)
           .FirstOrDefault().Stock;

        if (topPerformer is null)
        {
            return Result.Failure<StockPriceResponse>(StockErrors.NotFound("N/A"));
        }

        return Result.Success(topPerformer);
    }
}
