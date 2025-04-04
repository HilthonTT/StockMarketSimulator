using Application.Abstractions.Messaging;
using Modules.Budgeting.Contracts.Budgets;

namespace Modules.Budgeting.Application.Budgets.GetByUserId;

public sealed record GetBudgetByUserIdQuery(Guid UserId) : IQuery<BudgetResponse>;
