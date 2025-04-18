using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Url;
using Modules.Stocks.Contracts.Shorten;
using Modules.Stocks.Domain.Errors;
using SharedKernel;

namespace Modules.Stocks.Application.Shorten.Get;

internal sealed class GetShortenUrlQueryHandler(IUrlShorteningService urlShorteningService) 
    : IQueryHandler<GetShortenUrlQuery, ShortenUrlResponse>
{
    public async Task<Result<ShortenUrlResponse>> Handle(GetShortenUrlQuery request, CancellationToken cancellationToken)
    {
        string? originalUrl = await urlShorteningService.GetOriginalUrlAsync(request.ShortCode, cancellationToken);

        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            return Result.Failure<ShortenUrlResponse>(ShortenedUrlErrors.NotFound(request.ShortCode));
        }

        return new ShortenUrlResponse(originalUrl);
    }
}
