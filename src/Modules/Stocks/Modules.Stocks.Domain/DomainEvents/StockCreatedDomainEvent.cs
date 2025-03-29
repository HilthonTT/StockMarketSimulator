using SharedKernel;

namespace Stocks.Domain.DomainEvents;

public sealed record StockCreatedDomainEvent(Guid Id) : IDomainEvent;
