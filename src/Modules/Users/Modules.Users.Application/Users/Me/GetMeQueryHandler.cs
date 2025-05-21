using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Application.Users.Me;

internal sealed class GetMeQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext) : IQueryHandler<GetMeQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        Option<UserResponse> optionUser = await UserQueries.GetByIdAsync(connection, userContext.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(userContext.UserId));
        }

        UserResponse user = optionUser.ValueOrThrow();

        return user;
    }
}
