using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Url;
using Modules.Stocks.Contracts.Shorten;
using Modules.Stocks.Domain.Errors;
using SharedKernel;

namespace Modules.Stocks.Application.Shorten.GetByTicker;

internal sealed class GetShortenUrlByTickerQueryHandler(IUrlShorteningService urlShorteningService) 
    : IQueryHandler<GetShortenUrlByTickerQuery, ShortenUrlResponse>
{
    private static readonly HttpClient _httpClient = new();

    public async Task<Result<ShortenUrlResponse>> Handle(
        GetShortenUrlByTickerQuery request, 
        CancellationToken cancellationToken)
    {
        string? originalUrl = await urlShorteningService.GetOriginalUrlAsync(request.Ticker, cancellationToken);

        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            string generatedUrl = $"https://finance.yahoo.com/quote/{request.Ticker}";

            // Verify the URL exists before using it
            if (await UrlExistsAsync(generatedUrl, cancellationToken))
            {
                originalUrl = generatedUrl;
                await urlShorteningService.ShortenUrlAsync(request.Ticker, originalUrl, cancellationToken);
            }
            else
            {
                return Result.Failure<ShortenUrlResponse>(ShortenedUrlErrors.NotFound(request.Ticker));
            }
        }

        return new ShortenUrlResponse(originalUrl);
    }

    private static async Task<bool> UrlExistsAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(
                request, 
                HttpCompletionOption.ResponseHeadersRead, 
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
