﻿using Polly;
using Polly.Retry;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class RetryDelegatingHandler : DelegatingHandler
{
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .RetryAsync(2);

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
