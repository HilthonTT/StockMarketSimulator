using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Budgets.Reset;

/// <summary>Starts the account over: starting cash, no positions, no history, no open orders.</summary>
public sealed record ResetBudgetCommand(Guid UserId) : ICommand;
