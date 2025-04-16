namespace Infrastructure.Outbox;

public readonly struct OutboxUpdate
{
    public Guid Id { get; init; }

    public DateTime ProcessedOnUtc { get; init; }

    public string? Error { get; init; }
}
