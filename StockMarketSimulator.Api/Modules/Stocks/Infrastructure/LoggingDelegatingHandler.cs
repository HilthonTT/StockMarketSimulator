using System.Diagnostics;
using System.Net.Http.Headers;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;

    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Sending HTTP request: {Method} {Url}", request.Method, request.RequestUri);

            if (request.Content is not null)
            {
                string requestContent = await request.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogDebug("Request Content: {RequestContent}", requestContent);
            }

            LogHeaders("Request Headers", request.Headers);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("Received HTTP response: {StatusCode} {Url} (Elapsed: {ElapsedMilliseconds}ms)",
                response.StatusCode, request.RequestUri, stopwatch.ElapsedMilliseconds);

            LogHeaders("Response Headers", response.Headers);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("HTTP request failed: {StatusCode} {ReasonPhrase}. Response Content: {ErrorContent}",
                    response.StatusCode, response.ReasonPhrase, errorContent);
            }

            return response;
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            _logger.LogError(e, "HTTP request failed after {ElapsedMilliseconds}ms: {Method} {Url}",
                stopwatch.ElapsedMilliseconds, request.Method, request.RequestUri);
            throw;
        }
    }

    private void LogHeaders(string message, HttpHeaders headers)
    {
        if (headers is null || !headers.Any())
        {
            return;
        }

        List<string> formattedHeaders = [.. headers.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}")];

        _logger.LogDebug("{Message}:\n{Headers}", message, string.Join(Environment.NewLine, formattedHeaders));
    }
}
