using System.Reflection;
using Application.Abstractions.Data;
using Infrastructure.DomainEvents;
using Infrastructure.Outbox;
using Microsoft.Extensions.Logging;
using Modules.Users.Domain;
using SharedKernel;

namespace Modules.Users.Infrastructure.Outbox;

/// <summary>Processes the <c>users.outbox_messages</c> table. See <see cref="OutboxProcessorJob"/>.</summary>
public sealed class ProcessUserOutboxMessagesJob(
    IDbConnectionFactory dbConnectionFactory,
    IDomainEventsDispatcher domainEventsDispatcher,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessUserOutboxMessagesJob> logger)
    : OutboxProcessorJob(dbConnectionFactory, domainEventsDispatcher, dateTimeProvider, logger)
{
    public const string Name = nameof(ProcessUserOutboxMessagesJob);

    protected override string Schema => "users";

    protected override Assembly DomainAssembly => UsersDomainAssembly.Instance;
}
