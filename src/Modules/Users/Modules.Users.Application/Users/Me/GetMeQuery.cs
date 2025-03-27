using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Users;

namespace Modules.Users.Application.Users.Me;

public sealed record GetMeQuery : IQuery<UserResponse>;
