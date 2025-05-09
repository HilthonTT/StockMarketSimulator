using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(IDbConnectionFactory dbConnectionFactory) 
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        Option<UserResponse> optionUser = await UserQueries.GetByIdAsync(connection, request.UserId);
        if (!optionUser.IsSome)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
        }

        UserResponse user = optionUser.ValueOrThrow();

        return user;
    }
}
