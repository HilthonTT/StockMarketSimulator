using System.Reflection;
using Application.Abstractions.Data;
using Infrastructure.DomainEvents;
using Infrastructure.Outbox;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Domain;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Outbox;

/// <summary>Processes the <c>stocks.outbox_messages</c> table. See <see cref="OutboxProcessorJob"/>.</summary>
public sealed class ProcessStockOutboxMessagesJob(
    IDbConnectionFactory dbConnectionFactory,
    IDomainEventsDispatcher domainEventsDispatcher,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessStockOutboxMessagesJob> logger)
    : OutboxProcessorJob(dbConnectionFactory, domainEventsDispatcher, dateTimeProvider, logger)
{
    public const string Name = nameof(ProcessStockOutboxMessagesJob);

    protected override string Schema => "stocks";

    protected override Assembly DomainAssembly => StocksDomainAssembly.Instance;
}
