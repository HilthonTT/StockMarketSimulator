using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Abstractions.Url;
using Modules.Stocks.Contracts.Shorten;
using SharedKernel;

namespace Modules.Stocks.Application.Shorten.Create;

internal sealed class CreateShortenUrlCommandHandler(IUrlShorteningService urlShorteningService) 
    : ICommandHandler<CreateShortenUrlCommand, ShortenUrlResponse>
{
    public async Task<Result<ShortenUrlResponse>> Handle(
        CreateShortenUrlCommand request, 
        CancellationToken cancellationToken)
    {
        string shortCode = await urlShorteningService.ShortenUrlAsync(request.Url, cancellationToken);

        return new ShortenUrlResponse(shortCode);
    }
}
