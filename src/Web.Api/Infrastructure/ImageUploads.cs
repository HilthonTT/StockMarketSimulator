namespace Web.Api.Infrastructure;

/// <summary>
/// Rules for user-uploaded images.
/// </summary>
/// <remarks>
/// Uploads are served back inline from the API origin, so only raster formats that can't carry script are
/// accepted (no SVG, which can embed <c>&lt;script&gt;</c>). The declared content type must be on the allow-list
/// <em>and</em> match the file's signature bytes, because the header is client-controlled.
/// </remarks>
internal static class ImageUploads
{
    public const string InvalidImageMessage = "Only PNG, JPEG, GIF or WebP images are allowed.";

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Gif = "GIF8"u8.ToArray();
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp"
    };

    public static bool IsAllowedContentType(string? contentType) =>
        contentType is not null && AllowedContentTypes.Contains(contentType);

    /// <summary>Checks the declared content type and the file's signature.</summary>
    public static async Task<bool> IsValidAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (!IsAllowedContentType(file.ContentType) || file.Length == 0)
        {
            return false;
        }

        byte[] header = new byte[12];
        await using Stream stream = file.OpenReadStream();
        int read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);

        return MatchesSignature(file.ContentType, header.AsSpan(0, read));
    }

    public static bool MatchesSignature(string contentType, ReadOnlySpan<byte> header) =>
        contentType.ToLowerInvariant() switch
        {
            "image/png" => header.StartsWith(Png),
            "image/jpeg" => header.StartsWith(Jpeg),
            "image/gif" => header.StartsWith(Gif),
            "image/webp" => header.Length >= 12 && header.StartsWith(Riff) && header[8..12].SequenceEqual(Webp),
            _ => false
        };
}
