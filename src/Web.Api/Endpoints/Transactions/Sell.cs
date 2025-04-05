using MediatR;
using Modules.Budgeting.Application.Transactions.Sell;
using Modules.Budgeting.Contracts.Transactions;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Transactions;

internal sealed class Sell : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("transactions/sell", async (
            SellTransactionRequest request,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new SellTransactionCommand(request.UserId, request.Ticker, request.Quantity);

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Transactions)
        .HasPermission(Permission.Read, Permission.Write);
    }
}
