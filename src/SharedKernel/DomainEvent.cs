namespace SharedKernel;

public abstract record DomainEvent(Guid Id) : IDomainEvent;
