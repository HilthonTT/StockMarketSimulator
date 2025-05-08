using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Api.Api;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Errors;
using SharedKernel;

namespace Modules.Stocks.Application.Stocks.GetPurchasedStockTickers;

internal sealed class GetPurchasedStockTickersQueryHandler(
    IUserContext userContext,
    IBudgetingApi budgetingApi) : IQueryHandler<GetPurchasedStockTickersQuery, PurchasedStockTickersResponse>
{
    public async Task<Result<PurchasedStockTickersResponse>> Handle(
        GetPurchasedStockTickersQuery request, 
        CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<PurchasedStockTickersResponse>(UserErrors.Unauthorized);
        }

        List<string> tickers = await budgetingApi.GetPurchasedTickersAsync(userContext.UserId, cancellationToken);

        return new PurchasedStockTickersResponse(tickers);
    }
}
