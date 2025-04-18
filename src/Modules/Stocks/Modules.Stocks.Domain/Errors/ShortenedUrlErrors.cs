using SharedKernel;

namespace Modules.Stocks.Domain.Errors;

public static class ShortenedUrlErrors
{
    public static Error NotFound(string shortCode) => Error.NotFound(
        "ShortenedUrl.NotFound",
        $"The shortened with the short code = '{shortCode}' was not found");
}
