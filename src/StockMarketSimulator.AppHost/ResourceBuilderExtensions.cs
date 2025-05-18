using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StockMarketSimulator.AppHost;

internal static class ResourceBuilderExtensions
{
    internal static IResourceBuilder<T> WithSwaggerUI<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs("swagger-ui-docs", "Swagger API Documentation", "swagger");
    }

    internal static IResourceBuilder<T> WithScalar<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs("scalar-docs", "Scalar API Documentation", "scalar/v1");
    }

    internal static IResourceBuilder<T> WithRedoc<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs("redoc-docs", "Redoc API Documentation", "api-docs");
    }

    private static IResourceBuilder<T> WithOpenApiDocs<T>(
        this IResourceBuilder<T> builder,
        string name,
        string displayName,
        string openApiUiPath)
        where T : IResourceWithEndpoints
    {
        return builder.WithCommand(
            name,
            displayName,
            executeCommand: async _ =>
            {
                try
                {
                    // Base URL
                    EndpointReference endpoint = builder.GetEndpoint("https");

                    string url = $"{endpoint.Url}/{openApiUiPath}";

                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

                    return new ExecuteCommandResult { Success = true };
                }
                catch (Exception e)
                {
                    return new ExecuteCommandResult { Success = false, ErrorMessage = e.ToString() };
                }
            },
            new CommandOptions
            {
                UpdateState = context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy ?
                    ResourceCommandState.Enabled : ResourceCommandState.Disabled,
                IconName = "Document",
                IconVariant = IconVariant.Filled
            });
    }
}
