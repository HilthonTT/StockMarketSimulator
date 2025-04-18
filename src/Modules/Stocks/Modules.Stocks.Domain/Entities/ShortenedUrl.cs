using SharedKernel;

namespace Modules.Stocks.Domain.Entities;

public sealed class ShortenedUrl : Entity, IAuditable
{
    private ShortenedUrl()
    {
    }

    private ShortenedUrl(Guid id, string shortCode, string originalUrl)
    {
        Id = id;
        ShortCode = shortCode;
        OriginalUrl = originalUrl;
    }

    public Guid Id { get; init; }

    public string ShortCode { get; init; } = string.Empty;

    public string OriginalUrl { get; init; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static ShortenedUrl Create(string shortCode, string originalUrl)
    {
        return new ShortenedUrl(Guid.CreateVersion7(), shortCode, originalUrl);
    }
}
