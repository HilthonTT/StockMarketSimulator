using System.Net;
using System.Text.Json;
using Api.FunctionalTests.Abstractions;

namespace Api.FunctionalTests.OpenApi;

public sealed class OpenApiDocumentTests : BaseFunctionalTest
{
    public OpenApiDocumentTests(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task OpenApiDocument_Should_DescribeVersionedEndpoints()
    {
        HttpResponseMessage response = await Client.GetAsync("openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        List<string> paths = document.RootElement.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();

        // The API version is substituted into the route template, not left as a {version} parameter.
        Assert.Contains("/api/v1/authentication/register", paths);
        Assert.DoesNotContain(paths, path => path.Contains("{version", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SwaggerDocument_Should_BeServed()
    {
        HttpResponseMessage response = await Client.GetAsync("swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
