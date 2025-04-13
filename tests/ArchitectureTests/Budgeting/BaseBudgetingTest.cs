using Modules.Budgeting.Application.Budgets.GetByUserId;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Infrastructure.Outbox;
using System.Reflection;

namespace ArchitectureTests.Budgeting;

public abstract class BaseBudgetingTest
{
    protected static readonly Assembly DomainAssembly = typeof(Budget).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(GetBudgetByUserIdQuery).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ProcessBudgetingOutboxMessagesJob).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
