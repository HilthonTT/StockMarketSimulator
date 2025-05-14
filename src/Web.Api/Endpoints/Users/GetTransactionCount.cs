using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Transactions.GetTransactionCount;
using Modules.Budgeting.Contracts.Transactions;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetTransactionCount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/transaction-count", async (
            Guid userId,
            IQueryHandler<GetTransactionCountQuery, TransactionCountResponse> handler,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetTransactionCountQuery(userId))
                .Bind(query => handler.Handle(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
