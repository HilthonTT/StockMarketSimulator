using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;

namespace StockMarketSimulator.Api.Modules.Users.Application.GetCurrent;

internal sealed record GetCurrentUserQuery() : IQuery<UserResponse>;
