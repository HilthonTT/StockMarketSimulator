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

        List<TransactionApiResponse> buyTransactions = [.. transactions
            .Where(t => t.Type == 1) // TransactionType.Buy
            .GroupBy(t => t.Ticker)
            .Select(g => g.OrderByDescending(t => t.LimitPrice).First())];

        List<(StockPriceResponse Stock, decimal Performance)> performances = [];

        foreach (TransactionApiResponse tx in buyTransactions)
        {
            Option<StockPriceResponse> stockOption = await stockService.GetLatestStockPriceAsync(
                tx.Ticker, 
                cancellationToken);

            if (!stockOption.IsSome)
            {
                continue;
            }

            StockPriceResponse stock = stockOption.ValueOrThrow();

            decimal performance = (stock.Price - tx.LimitPrice) / tx.LimitPrice;
            performances.Add((stock, performance));
        }

        StockPriceResponse topPerformer = performances
           .OrderByDescending(p => p.Performance)
           .FirstOrDefault().Stock;

        return Result.Success(topPerformer);
    }
}
