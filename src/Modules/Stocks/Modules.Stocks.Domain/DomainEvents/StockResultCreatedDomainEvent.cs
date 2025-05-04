using SharedKernel;

namespace Modules.Stocks.Domain.DomainEvents;

public sealed record StockResultCreatedDomainEvent(Guid Id, Guid StockResultId) : DomainEvent(Id);
