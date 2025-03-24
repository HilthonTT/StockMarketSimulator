using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;

namespace StockMarketSimulator.Api.Modules.Users.Application.GetById;

internal sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;
