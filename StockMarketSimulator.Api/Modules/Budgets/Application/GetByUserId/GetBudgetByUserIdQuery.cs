using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Budgets.Contracts;

namespace StockMarketSimulator.Api.Modules.Budgets.Application.GetByUserId;

internal sealed record GetBudgetByUserIdQuery(Guid UserId) : IQuery<BudgetResponse>;
