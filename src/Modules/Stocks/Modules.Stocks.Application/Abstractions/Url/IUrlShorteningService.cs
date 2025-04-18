using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Application.Abstractions.Url;

public interface IUrlShorteningService
{
    Task<string> ShortenUrlAsync(string originalUrl, CancellationToken cancellationToken = default);

    Task<string?> GetOriginalUrlAsync(string shortCode, CancellationToken cancellationToken = default);

    Task<IEnumerable<ShortenedUrl>> GetAllUrlsAsync(CancellationToken cancellationToken = default);
}
