using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Users.Application.RevokeRefreshTokens;

internal sealed record RevokeRefreshTokensCommand(Guid UserId) : ICommand;
