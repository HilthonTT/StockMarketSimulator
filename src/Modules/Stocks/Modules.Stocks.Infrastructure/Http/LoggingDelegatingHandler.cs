using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Modules.Stocks.Infrastructure.Http;

internal sealed class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
       HttpRequestMessage request,
       CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Sending HTTP request: {Method} {Url}", request.Method, Redact(request.RequestUri));

            if (request.Content is not null)
            {
                string requestContent = await request.Content.ReadAsStringAsync(cancellationToken);

                logger.LogDebug("Request Content: {RequestContent}", requestContent);
            }

            LogHeaders("Request Headers", request.Headers);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            logger.LogInformation("Received HTTP response: {StatusCode} {Url} (Elapsed: {ElapsedMilliseconds}ms)",
                response.StatusCode, Redact(request.RequestUri), stopwatch.ElapsedMilliseconds);

            LogHeaders("Response Headers", response.Headers);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                logger.LogWarning("HTTP request failed: {StatusCode} {ReasonPhrase}. Response Content: {ErrorContent}",
                    response.StatusCode, response.ReasonPhrase, errorContent);
            }

            return response;
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            logger.LogError(e, "HTTP request failed after {ElapsedMilliseconds}ms: {Method} {Url}",
                stopwatch.ElapsedMilliseconds, request.Method, Redact(request.RequestUri));
            throw;
        }
    }

    /// <summary>
    /// Strips the query string before logging: it carries the API key (and user search terms).
    /// </summary>
    private static string? Redact(Uri? uri) =>
        uri is null ? null : uri.IsAbsoluteUri ? uri.GetLeftPart(UriPartial.Path) : uri.OriginalString.Split('?')[0];

    private void LogHeaders(string message, HttpHeaders headers)
    {
        if (headers is null || !headers.Any())
        {
            return;
        }

        List<string> formattedHeaders = [.. headers.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}")];

        logger.LogDebug("{Message}:\n{Headers}", message, string.Join(Environment.NewLine, formattedHeaders));
    }
}
