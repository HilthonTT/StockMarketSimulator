using SharedKernel;
using StockMarketSimulator.Api.Endpoints;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Modules.Transactions.Application.Buy;
using StockMarketSimulator.Api.Modules.Transactions.Application.GetById;
using StockMarketSimulator.Api.Modules.Transactions.Application.GetByUserId;
using StockMarketSimulator.Api.Modules.Transactions.Application.Sell;
using StockMarketSimulator.Api.Modules.Transactions.Contracts;

namespace StockMarketSimulator.Api.Modules.Transactions.Presentation;

internal sealed class TransactionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("transactions/{transactionId}", async (
            Guid transactionId,
            GetTransactionByIdQueryHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTransactionByIdQuery(transactionId);

            Result<TransactionResponse> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Transactions)
        .RequireAuthorization();

        app.MapGet("users/{userId}/transactions", async (
            Guid userId,
            GetTransactionByUserIdQueryHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTransactionsByUserIdQuery(userId);

            Result<List<TransactionResponse>> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Transactions)
        .RequireAuthorization();

        app.MapPost("transactions/buy", async (
            BuyTransactionRequest request,
            BuyTransactionCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new BuyTransactionCommand(request.Ticker, request.LimitPrice, request.Quantity);

            Result<Guid> result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Transactions)
        .RequireAuthorization();

        app.MapPost("transactions/sell", async (
            SellTransactionRequest request,
            SellTransactionCommandHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var command = new SellTransactionCommand(request.Ticker, request.Quantity);

            Result<Guid> result = await useCase.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Transactions)
        .RequireAuthorization();
    }
}
