namespace Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ProcessedOnUtc { get; init; } = null;

    public string? Error { get; init; } = null;
}
