using SharedKernel;

namespace Modules.Stocks.Domain.DomainEvents;

public sealed record StockCreatedDomainEvent(Guid Id, Guid StockId) : DomainEvent(Id);
