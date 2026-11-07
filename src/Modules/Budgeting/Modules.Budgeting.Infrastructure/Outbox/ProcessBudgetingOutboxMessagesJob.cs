using System.Reflection;
using Application.Abstractions.Data;
using Infrastructure.DomainEvents;
using Infrastructure.Outbox;
using Microsoft.Extensions.Logging;
using Modules.Budgeting.Domain;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Outbox;

/// <summary>Processes the <c>budgeting.outbox_messages</c> table. See <see cref="OutboxProcessorJob"/>.</summary>
public sealed class ProcessBudgetingOutboxMessagesJob(
    IDbConnectionFactory dbConnectionFactory,
    IDomainEventsDispatcher domainEventsDispatcher,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessBudgetingOutboxMessagesJob> logger)
    : OutboxProcessorJob(dbConnectionFactory, domainEventsDispatcher, dateTimeProvider, logger)
{
    public const string Name = nameof(ProcessBudgetingOutboxMessagesJob);

    protected override string Schema => "budgeting";

    protected override Assembly DomainAssembly => BudgetingDomainAssembly.Instance;
}
