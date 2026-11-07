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

        // Cost basis comes from the Budgeting positions (average-cost method), maintained with every trade.
        List<PositionApiResponse> openPositions = await budgetingApi.GetOpenPositionsAsync(
            userContext.UserId,
            cancellationToken);

        List<(StockPriceResponse Stock, decimal Performance)> performances = [];

        foreach ((string ticker, _, decimal averageCost, _) in openPositions.Where(p => p.AverageCost > 0))
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
