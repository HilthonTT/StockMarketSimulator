using SharedKernel;

namespace Stocks.Domain.DomainEvents;

public sealed record StockResultCreatedDomainEvent(Guid Id) : IDomainEvent;
