using Polly.Retry;
using Polly;

namespace Modules.Stocks.Infrastructure.Http;

internal sealed class RetryDelegatingHandler : DelegatingHandler
{
    private const int RetryCount = 3;

    private static readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
       Policy<HttpResponseMessage>
           .Handle<HttpRequestException>()
           .RetryAsync(RetryCount);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        PolicyResult<HttpResponseMessage> policyResult = await _retryPolicy.ExecuteAndCaptureAsync(
            () => base.SendAsync(request, cancellationToken));

        if (policyResult.Outcome == OutcomeType.Failure)
        {
            throw new HttpRequestException("Something went wrong", policyResult.FinalException);
        }

        return policyResult.Result;
    }
}
