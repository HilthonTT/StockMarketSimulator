using Web.Api.Infrastructure;

namespace Application.UnitTests.Web;

public sealed class ImageUploadsTests
{
    [Theory]
    [InlineData("image/png", true)]
    [InlineData("IMAGE/JPEG", true)]
    [InlineData("image/gif", true)]
    [InlineData("image/webp", true)]
    [InlineData("image/svg+xml", false)]
    [InlineData("text/html", false)]
    [InlineData(null, false)]
    public void IsAllowedContentType_Should_OnlyAcceptRasterFormats(string? contentType, bool expected)
    {
        Assert.Equal(expected, ImageUploads.IsAllowedContentType(contentType));
    }

    [Fact]
    public void MatchesSignature_Should_AcceptRealHeaders()
    {
        Assert.True(ImageUploads.MatchesSignature("image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0]));
        Assert.True(ImageUploads.MatchesSignature("image/jpeg", [0xFF, 0xD8, 0xFF, 0xE0]));
        Assert.True(ImageUploads.MatchesSignature("image/gif", "GIF89a"u8));
        Assert.True(ImageUploads.MatchesSignature("image/webp", "RIFF\0\0\0\0WEBP"u8));
    }

    [Fact]
    public void MatchesSignature_Should_RejectSvgDeclaredAsPng()
    {
        Assert.False(ImageUploads.MatchesSignature("image/png", "<svg xmlns=\"http://www.w3.org/2000/svg\">"u8));
        Assert.False(ImageUploads.MatchesSignature("image/webp", "RIFF\0\0\0\0WAVE"u8));
    }
}
