using Application.Abstractions.Caching;
using Modules.Users.Contracts.Users;

namespace Modules.Users.Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : ICachedQuery<UserResponse>
{
    public string CacheKey => $"users:{UserId}";

    public TimeSpan? Expiration => null;
}
