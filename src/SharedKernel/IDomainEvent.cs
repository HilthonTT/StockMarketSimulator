namespace SharedKernel;

public interface IDomainEvent
{
    public Guid Id { get; init; }
}
