using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Users.Application.ChangePassword;

internal sealed record ChangeUserPasswordCommand(
    Guid UserId, 
    string CurrentPassword, 
    string NewPassword) : ICommand;
